# Gamify API Documentation

A comprehensive e-commerce API for gaming products with authentication, cart management, and order processing.

## 🚀 Getting Started

### Prerequisites
- .NET 9.0
- PostgreSQL Database
- RabbitMQ (for order processing)
- Cloudinary (for image uploads)

### Installation

1. Clone the repository
2. Install dependencies:
   ```bash
   dotnet restore
   ```
3. Set up environment variables in `.env` file:
   ```
   DATABASE_CONNECTION_STRING=your_postgres_connection_string
   SECRET_KEY=your_jwt_secret_key
   CLOUDINARY_CLOUD_NAME=your_cloudinary_cloud_name
   CLOUDINARY_API_KEY=your_cloudinary_api_key
   CLOUDINARY_API_SECRET=your_cloudinary_api_secret
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## 📚 API Documentation

### Swagger UI
Once the application is running, you can access the interactive API documentation at:
```
http://localhost:3000/swagger
```

The Swagger UI provides:
- Complete endpoint documentation
- Request/response examples
- Interactive testing interface
- Authentication with Bearer tokens

### Authentication

The API uses JWT Bearer token authentication. To authenticate:

1. **Register a new user** using the `/api/v1/auth/register` endpoint
2. **Login** using the `/api/v1/auth/login` endpoint to get a JWT token
3. **Include the token** in the Authorization header for protected endpoints:
   ```
   Authorization: Bearer your_jwt_token_here
   ```

## 🔐 API Endpoints

### Authentication (`/api/v1/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/register` | Register a new user account | No |
| POST | `/login` | Authenticate user and get JWT token | No |
| GET | `/profile` | Get current user profile | Yes |
| GET | `/users` | Get all users (Admin only) | Yes (Admin) |

### Cart Management (`/api/v1/cart`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/getItems` | Get all items in user's cart | Yes |
| POST | `/add` | Add product to cart | Yes |
| POST | `/quantity` | Change product quantity or remove | Yes |

### Categories (`/api/v1/category`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/all` | Get all product categories | No |
| POST | `/create` | Create new category | Yes (Admin) |

### Products (`/api/v1/product`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/all` | Get all products with pagination/filtering | No |
| POST | `/create` | Create new product | Yes (Seller) |
| POST | `/{productId}/generate-presigned-url` | Generate image upload URLs | Yes |

### Orders (`/api/v1/order`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/create` | Create new order | Yes |

### Addresses (`/api/v1/address`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/add` | Add new address for user | Yes |

## 📋 Request/Response Examples

### Register User
```json
POST /api/v1/auth/register
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "password123",
  "profilePictureUrl": "https://example.com/avatar.jpg",
  "city": "New York",
  "state": "NY",
  "country": "USA"
}
```

### Login
```json
POST /api/v1/auth/login
{
  "email": "john@example.com",
  "password": "password123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Create Product
```json
POST /api/v1/product/create
Authorization: Bearer your_token_here

{
  "title": "Gaming Mouse",
  "description": "High-performance gaming mouse with RGB lighting",
  "price": 5999,
  "stock": 50,
  "categoryId": "category-uuid"
}
```

### Add to Cart
```json
POST /api/v1/cart/add
Authorization: Bearer your_token_here

{
  "productId": "product-uuid",
  "quantity": 2
}
```

## 🔧 Query Parameters

### Product Filtering and Pagination
```
GET /api/v1/product/all?pageNumber=1&pageSize=10&categoryId=uuid&sortBy=price&sortOrder=desc
```

Parameters:
- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `categoryId`: Filter by category (optional)
- `sortBy`: Sort field - `price`, `title`, or `createdAt` (default: `createdAt`)
- `sortOrder`: Sort order - `asc` or `desc` (default: `desc`)

## 🛡️ Security

### Roles and Permissions
- **User**: Can view products, manage cart, create orders, manage addresses
- **Seller**: Can create and manage products
- **Admin**: Can manage categories and view all users

### Protected Endpoints
Most endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🚨 Error Handling

The API returns appropriate HTTP status codes:

- `200`: Success
- `400`: Bad Request (invalid data)
- `401`: Unauthorized (authentication required)
- `403`: Forbidden (insufficient permissions)
- `404`: Not Found
- `409`: Conflict (resource already exists)
- `500`: Internal Server Error

Error responses include descriptive messages:
```json
{
  "message": "Invalid product data. Title, Description, Price, Stock, and CategoryId are required."
}
```

## 📝 Usage Examples

### Using Swagger UI
1. Navigate to `http://localhost:3000/swagger`
2. Click "Authorize" and enter your Bearer token
3. Explore endpoints by category (Auth, Cart, Category, etc.)
4. Test endpoints directly from the UI

### Using cURL
```bash
# Login
curl -X POST "http://localhost:3000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'

# Get products (with token)
curl -X GET "http://localhost:3000/api/v1/product/all" \
  -H "Authorization: Bearer your_token_here"
```

## 🔄 Development

### Running in Development Mode
```bash
dotnet run --environment Development
```

### Database Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## 📞 Support

For API support, contact: support@gamify.com

## 📄 License

This project is licensed under the MIT License. 