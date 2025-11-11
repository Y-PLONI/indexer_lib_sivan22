using System;
using System.Runtime.InteropServices;
using System.Linq;
using IndexerLib.Index;
using IndexerLib.IndexSearch;

namespace IndexerLibWrapper
{
    public class IndexerWrapper
    {
        // Structure to return search results
        [StructLayout(LayoutKind.Sequential)]
        public struct SearchResultItem
        {
            public int DocId;
            public int MatchCount;
        }

        [UnmanagedCallersOnly(EntryPoint = "indexer_create_index")]
        public static unsafe int CreateIndex(byte* directoryPtr, byte* extensionsPtr, int memoryUsage)
        {
            try
            {
                if (directoryPtr == null || extensionsPtr == null)
                    return -1;

                string directory = Marshal.PtrToStringUTF8((IntPtr)directoryPtr);
                string extensionsStr = Marshal.PtrToStringUTF8((IntPtr)extensionsPtr);
                
                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(extensionsStr))
                    return -1;

                // Parse extensions (comma-separated)
                string[] extensions = extensionsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToArray();

                IndexCreator.Execute(directory, extensions, memoryUsage);
                return 0; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating index: {ex.Message}");
                return -1; // Error
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "indexer_search")]
        public static unsafe int Search(byte* queryPtr, short adjacency, IntPtr resultsBuffer, int maxResults)
        {
            try
            {
                if (queryPtr == null)
                    return -1;

                string query = Marshal.PtrToStringUTF8((IntPtr)queryPtr);
                if (string.IsNullOrEmpty(query))
                    return -1;

                var results = StreamingSearch.Execute(query, adjacency).Take(maxResults).ToArray();
                
                if (results.Length == 0)
                    return 0;

                // Marshal results to buffer
                var resultItems = results.Select(r => new SearchResultItem
                {
                    DocId = r.DocId,
                    MatchCount = r.MatchedPostings.Count
                }).ToArray();

                if (resultsBuffer != IntPtr.Zero)
                {
                    int structSize = Marshal.SizeOf<SearchResultItem>();
                    for (int i = 0; i < resultItems.Length; i++)
                    {
                        IntPtr itemPtr = IntPtr.Add(resultsBuffer, i * structSize);
                        Marshal.StructureToPtr(resultItems[i], itemPtr, false);
                    }
                }

                return results.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching: {ex.Message}");
                return -1;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "indexer_get_doc_path")]
        public static unsafe byte* GetDocPath(int docId)
        {
            try
            {
                using (var docIdStore = new DocIdStore())
                {
                    var path = docIdStore.GetPathById(docId);
                    if (string.IsNullOrEmpty(path))
                        return null;

                    return (byte*)Marshal.StringToCoTaskMemUTF8(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doc path: {ex.Message}");
                return null;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "indexer_free_string")]
        public static unsafe void FreeString(byte* ptr)
        {
            if (ptr != null)
            {
                Marshal.FreeCoTaskMem((IntPtr)ptr);
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "indexer_get_snippet")]
        public static unsafe byte* GetSnippet(int docId, byte* queryPtr)
        {
            try
            {
                if (queryPtr == null)
                    return null;

                string query = Marshal.PtrToStringUTF8((IntPtr)queryPtr);
                if (string.IsNullOrEmpty(query))
                    return null;

                // Create a search result to generate snippet
                var searchResult = StreamingSearch.Execute(query, 2)
                    .FirstOrDefault(r => r.DocId == docId);

                if (searchResult == null || searchResult.MatchedPostings.Count == 0)
                    return null;

                using (var docIdStore = new DocIdStore())
                {
                    SnippetBuilder.GenerateSnippets(searchResult, docIdStore, 100);
                    
                    if (searchResult.Snippets == null || searchResult.Snippets.Length == 0)
                        return null;

                    // Return the first snippet
                    var snippet = searchResult.Snippets[0];
                    return (byte*)Marshal.StringToCoTaskMemUTF8(snippet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting snippet: {ex.Message}");
                return null;
            }
        }
    }
}
