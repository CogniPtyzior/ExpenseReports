// Grants the backend test assembly access to internal application types.
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WebApi.Tests")]