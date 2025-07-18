# 🚀 Quick Start Guide

## Getting Started in Under 5 Minutes

### 1. Install Dependencies
```bash
npm install
```

### 2. Set Up Database
```bash
npm run db:setup
```

### 3. Start the Application
```bash
npm start
```

### 4. Open in Browser
Navigate to: **http://localhost:3000**

---

## What You'll See

### 🏠 Web Interface
- Clean Bootstrap form to input customer data
- Real-time credit score calculation
- Visual risk category indicators
- History of previous simulations

### 🔌 API Endpoints
- `POST /api/simulate` - Calculate new credit score
- `GET /api/simulations` - List all simulations
- `GET /api/simulation/:id` - Get specific simulation
- `GET /api/health` - Health check

### 📊 Sample Test
Try this customer data:
- **Name**: John Doe
- **Age**: 35
- **Annual Income**: $75,000
- **Debt-to-Income Ratio**: 0.25 (25%)
- **Loan Amount**: $30,000
- **Credit History**: Good

---

## Development Commands

```bash
# Run in development mode (auto-reload)
npm run dev

# Run tests
npm test

# Run tests in watch mode
npm run test:watch
```

---

## Quick API Test

```bash
# Test the simulation endpoint
curl -X POST http://localhost:3000/api/simulate \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Customer",
    "age": 30,
    "annualIncome": 60000,
    "debtToIncomeRatio": 0.3,
    "loanAmount": 25000,
    "creditHistory": "good"
  }'

# Get all simulations
curl http://localhost:3000/api/simulations

# Check health
curl http://localhost:3000/api/health
```

---

## 🎯 What This Demonstrates

✅ **Full-Stack Development**: Express API + SQLite + Bootstrap UI  
✅ **Business Logic**: Rule-based credit scoring algorithm  
✅ **Database Operations**: CRUD operations with SQLite  
✅ **API Design**: RESTful endpoints with validation  
✅ **Testing**: Comprehensive Jest test suite  
✅ **Documentation**: OpenAPI specification  
✅ **Security**: Input validation, SQL injection protection  
✅ **Frontend**: Responsive Bootstrap interface

---

## 📁 Project Structure

```
credit-risk-simulator/
├── src/
│   ├── app.js                 # Main Express app
│   ├── database/              # Database layer
│   ├── routes/                # API routes
│   └── services/              # Business logic
├── public/                    # Frontend assets
├── tests/                     # Test suites
├── docs/                      # API documentation
└── data/                      # SQLite database
```

---

**⚠️ Remember**: This is a demonstration application only!
