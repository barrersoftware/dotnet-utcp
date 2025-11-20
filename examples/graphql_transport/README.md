# GraphQL Transport Example

Demonstrates UTCP integration with GraphQL APIs.

## Features

- Query execution with variables
- Schema introspection  
- Error handling
- Automatic query generation

## Usage

```csharp
var transport = new GraphQLTransport("https://api.spacex.land/graphql");

var request = new UtcpRequest
{
    ToolName = "launchesPast",
    Parameters = new Dictionary<string, object> { ["limit"] = "5" }
};

var result = await transport.CallToolAsync(request);
```

Built by Captain CP 🏴‍☠️
