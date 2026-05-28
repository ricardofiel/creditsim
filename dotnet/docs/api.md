# API Reference

All endpoints are served under the `/api` prefix. Responses are always JSON with camelCase property names.

---

## POST /api/simulate

Calculates a credit score for the submitted customer data and persists the result.

**Original Node.js route:** `src/routes/simulation.js`, line 58

### Request Body

```json
{
  "name": "John Doe",
  "age": 35,
  "annualIncome": 60000,
  "debtToIncomeRatio": 0.3,
  "loanAmount": 25000,
  "creditHistory": "good"
}
```

| Field | Type | Constraints |
|---|---|---|
| `name` | string | 1–100 characters, required |
| `age` | integer | 18–120 |
| `annualIncome` | number | ≥ 0 |
| `debtToIncomeRatio` | number | 0–1 |
| `loanAmount` | number | > 0 |
| `creditHistory` | string | `"good"` or `"bad"` |

### Response — 201 Created

```json
{
  "id": 42,
  "score": 640,
  "riskCategory": "High risk",
  "message": "Credit score calculated successfully",
  "customer": {
    "name": "John Doe",
    "age": 35,
    "annualIncome": 60000,
    "debtToIncomeRatio": 0.3,
    "loanAmount": 25000,
    "creditHistory": "good"
  }
}
```

### Response — 400 Bad Request (validation failed)

```json
{
  "error": "Validation failed",
  "details": [
    { "field": "Name", "message": "Name must be between 1 and 100 characters" }
  ]
}
```

### Status Codes

| Code | Condition |
|---|---|
| 201 | Success |
| 400 | Validation error |
| 500 | Internal server error |

---

## GET /api/simulations

Returns all previous simulation records, ordered by `createdAt DESC`.

**Original Node.js route:** `src/routes/simulation.js`, line 104

### Response — 200 OK

```json
{
  "count": 2,
  "simulations": [
    {
      "id": 2,
      "name": "Jane Smith",
      "score": 710,
      "riskCategory": "Medium risk",
      "loanAmount": 15000,
      "createdAt": "2024-01-15T10:30:00.000Z"
    }
  ]
}
```

### Status Codes

| Code | Condition |
|---|---|
| 200 | Success |
| 500 | Internal server error |

---

## GET /api/simulation/{id}

Returns the full detail of a single simulation by its primary key.

**Original Node.js route:** `src/routes/simulation.js`, line 133

### Path Parameter

| Parameter | Type | Constraints |
|---|---|---|
| `id` | integer | ≥ 1 |

### Response — 200 OK

```json
{
  "simulation": {
    "id": 1,
    "name": "John Doe",
    "age": 35,
    "annualIncome": 60000,
    "debtToIncomeRatio": 0.3,
    "loanAmount": 25000,
    "creditHistory": "good",
    "score": 640,
    "riskCategory": "High risk",
    "createdAt": "2024-01-15T10:30:00.000Z"
  }
}
```

### Status Codes

| Code | Condition |
|---|---|
| 200 | Found |
| 400 | `id` is not a positive integer |
| 404 | No record with that `id` |
| 500 | Internal server error |

---

## GET /api/scoring-criteria

Returns the scoring model explanation (informational, read-only).

**Original Node.js route:** `src/routes/simulation.js`, line 173

### Response — 200 OK

```json
{
  "criteria": {
    "baseScore": 600,
    "adjustments": {
      "age":              { "under25": -50, "over60": -30 },
      "income":           { "over50k": 40 },
      "debtToIncomeRatio":{ "over40Percent": -80 },
      "creditHistory":    { "bad": -150 },
      "loanToIncomeRatio":{ "over50Percent": -50 }
    },
    "riskCategories": {
      "lowRisk":    "750+",
      "mediumRisk": "650-749",
      "highRisk":   "Below 650"
    }
  },
  "disclaimer": "This is a demonstration scoring model and should not be used for actual credit decisions."
}
```

---

## GET /api/health

Health-check endpoint.

**Original Node.js route:** `src/routes/simulation.js`, line 193

### Response — 200 OK

```json
{
  "status":    "healthy",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "uptime":    12345.67
}
```

> **Note:** `uptime` in Node.js is `process.uptime()` (seconds since Node process started).  
> In C#, it is `(DateTime.UtcNow - Process.StartTime.ToUniversalTime()).TotalSeconds`.

---

## Customer List Page

`GET /` → Redirects to `Default.aspx` (ASP.NET WebForm).

This is a **server-rendered** page (not a REST endpoint) that displays the customers table with server-side `GridView` paging (page size 10, ordered `createdAt DESC`).
