# Hotel Search API

Hotel Search API is a .NET web service that allows users to manage hotels and search for accommodations based on their geographic location. The application uses an intelligent ranking algorithm that considers both proximity and price to deliver the best hotel recommendations.

## Key Features

- **Full CRUD Operations** - Create, read, update, and delete hotel records
- **Intelligent Search Algorithm** - Combines distance and price for optimal hotel ranking
- **Pagination Support** - Efficient handling of large result sets
- **Geospatial Calculations** - Accurate distance computation using Haversine formula
- **Duplicate Prevention** - Case-insensitive validation prevents duplicate hotels at same location
- **Clean Architecture** - Separation of concerns with 4-layer architecture
- **Flexible Persistence** - Supports both in-memory and PostgreSQL storage
- **Interactive API Documentation** - Swagger/OpenAPI interface for easy testing
- **Production-Ready** - Comprehensive logging, health checks, and error handling
- **CI/CD Pipeline** - GitHub Actions workflow for automated build and testing
