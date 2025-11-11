# IndexerLib - Dart FFI Bindings

A Dart package providing FFI (Foreign Function Interface) bindings to IndexerLib, a high-performance full-text search engine written in C#. This package enables you to create searchable indexes of documents and perform fast, streaming-based searches with proximity matching.

## Features

- **Full-Text Indexing**: Create indexes from text and PDF files with customizable file extensions
- **Streaming Search**: Memory-efficient search that reads directly from index files without loading everything into RAM
- **Proximity Matching**: Find terms within a specified word distance (adjacency)
- **Document Management**: Map document IDs to file paths
- **Snippet Generation**: Extract highlighted text snippets around matching terms
- **Cross-Platform**: Supports Windows (tested), Linux, and macOS through Native AOT compilation
- **High Performance**: Built with Native AOT for optimal speed and minimal memory footprint

## Requirements

- **Dart SDK**: ^3.9.2
- **.NET SDK**: 8.0 or higher (for building the C# library)
- **Platform**: Windows (tested), Linux, or macOS

Android is currently not supported for the C# NativeAOT bridge.

## Building the Library

Before using the package, you must build the C# wrapper library:

### Windows
```bash
.\build.bat
```

### Linux/macOS
```bash
chmod +x build.sh
./build.sh
```

The build scripts will:
1. Compile the included IndexerLib sources with the FFI wrapper using Native AOT
2. Output the library to `csharp_lib/bin/Release/net8.0/{rid}/publish/` where `rid` is one of:
  - `win-x64`
  - `linux-x64`
  - `osx-x64` or `osx-arm64`

## Getting Started

1. Add the package to your `pubspec.yaml`:
```yaml
dependencies:
  indexer_lib:
    path: path/to/indexer_lib
  ffi: ^2.1.0
```

2. Build the native library (see Building the Library section above)

3. Run your Dart application from the publish directory to ensure all dependencies are found:
```bash
cd indexer_lib/csharp_lib/bin/Release/net8.0/win-x64/publish
dart your_app.dart
```

## Usage

### Basic Example

```dart
import 'package:indexer_lib/indexer_lib.dart';

void main() {
  // Initialize the indexer
  final indexer = IndexerLib();
  
  // Create an index for a directory
  final result = indexer.createIndex(
    'C:\\Documents',
    '.txt,.pdf',  // Comma-separated file extensions
    memoryUsage: 10,  // Memory limit in MB
  );
  
  if (result == 0) {
    print('Index created successfully!');
  }
  
  // Search the index
  final searchResults = indexer.search(
    'example query',
    adjacency: 2,  // Maximum word distance between terms
    maxResults: 100,
  );
  
  // Process results
  for (var result in searchResults) {
    print('Document ID: ${result.docId}, Matches: ${result.matchCount}');
    
    // Get the document path
    final path = indexer.getDocPath(result.docId);
    if (path != null) {
      print('Path: $path');
    }
    
    // Get a highlighted snippet
    final snippet = indexer.getSnippet(result.docId, 'example query');
    if (snippet != null) {
      print('Snippet: $snippet');
    }
  }
}
```

## API Reference

### IndexerLib Class

#### Constructor
```dart
IndexerLib()
```
Initializes the FFI bindings and loads the native library.

#### Methods

##### createIndex
```dart
int createIndex(String directory, String extensions, {int memoryUsage = 10})
```
Creates an index for the specified directory.

**Parameters:**
- `directory`: The directory to index
- `extensions`: Comma-separated list of file extensions (e.g., ".txt,.pdf")
- `memoryUsage`: Memory usage limit in MB (default: 10)

**Returns:** 
- `0` on success
- `-1` on error

##### search
```dart
List<SearchResult> search(String query, {int adjacency = 2, int maxResults = 100})
```
Searches the index for the given query.

**Parameters:**
- `query`: The search query
- `adjacency`: Maximum word distance between query terms (default: 2)
- `maxResults`: Maximum number of results to return (default: 100)

**Returns:** List of `SearchResult` objects

##### getDocPath
```dart
String? getDocPath(int docId)
```
Gets the file path for a document ID.

**Parameters:**
- `docId`: The document ID

**Returns:** The file path or `null` if not found

##### getSnippet
```dart
String? getSnippet(int docId, String query)
```
Gets a highlighted text snippet around the query terms.

**Parameters:**
- `docId`: The document ID
- `query`: The search query

**Returns:** A snippet with `<mark>` tags around matched terms, or `null` if not found

### SearchResult Class

Represents a search result.

**Properties:**
- `int docId`: The document identifier
- `int matchCount`: Number of matches found in the document

## Architecture

This package uses FFI to bridge Dart with a C# library compiled using Native AOT:

```
Dart Application
      ↓ (FFI)
IndexerLib Dart Wrapper (lib/src/indexer_lib_base.dart)
      ↓ (P/Invoke)
C# Wrapper (csharp_lib/IndexerLibWrapper.cs)
      ↓
IndexerLib sources (included in `csharp_lib/IndexerLib/**`)

All string interop uses UTF-8. Non-ASCII (e.g., Hebrew) paths and snippets are supported end-to-end.
```

The Native AOT compilation produces a self-contained native library with no .NET runtime dependencies, making it fast and efficient.

## Examples

See the `/example` directory for complete examples:
- `indexer_lib_example.dart`: Full-featured example
- `simple_test.dart`: Simple FFI verification test

## Troubleshooting

### "Unable to load DLL" errors
Make sure you're running your Dart application from the publish directory where all dependencies are located:
```bash
cd indexer_lib/csharp_lib/bin/Release/net8.0/win-x64/publish
dart path/to/your_app.dart
```

### "Search failed" errors
Ensure an index has been created before searching. The index is stored in the C:\ drive by default.

### Build errors
Ensure you have:
- .NET SDK 8.0 or higher installed
- All NuGet packages restored

On Linux/macOS, ensure you build on the target OS (cross-compiling from Windows is not supported by NativeAOT toolchain out of the box).

Android is not supported with this C# bridge. Consider an alternative native backend (C/C++/Rust) for Android, or .NET for Android NativeAOT with appropriate workloads and tooling.

## License

See LICENSE file for details.

## Contributing

This is a template package. Contributions and improvements are welcome!
