# Product Business Rules (BDD Style)

## 🎯 Overview

This document describes the business rules for Product management using **Behavior-Driven Development (BDD)** style specifications. Each rule is linked to its corresponding unit test implementation.

---

## Feature: Product Creation

### Business Context
As a **product manager**  
I want to **create products with validated business constraints**  
So that **all products in the system meet business standards**

---

## Rule 1: Price Validation

### Scenario: Valid product prices are accepted
```gherkin
Given I want to create a product
When I specify a price between $0.01 and $10,000
Then the product should be created successfully
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithValidPrice_ShouldCreateProduct`

### Scenario: Products cannot exceed maximum price limit
```gherkin
Given I want to create a product
When I specify a price greater than $10,000
Then the system should reject the product
And display the error "Price cannot exceed $10000"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithPriceExceeding10000_ShouldThrowInvalidPriceException`

### Scenario: Products cannot have negative prices
```gherkin
Given I want to create a product
When I specify a negative price
Then the system should reject the product
And display the error "Price must be greater than zero"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithNegativePrice_ShouldThrowInvalidPriceException`

### Scenario: Products cannot have zero price
```gherkin
Given I want to create a product
When I specify a price of exactly $0
Then the system should reject the product
And display the error "Price must be greater than zero"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithZeroPrice_ShouldThrowInvalidPriceException`

---

## Rule 2: Weight Validation

### Scenario: Valid product weights are accepted
```gherkin
Given I want to create a product
When I specify a weight between 0.01kg and 80kg
Then the product should be created successfully
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithValidWeight_ShouldCreateProduct`

### Scenario: Products cannot exceed maximum weight limit
```gherkin
Given I want to create a product
When I specify a weight greater than 80kg
Then the system should reject the product
And display the error "Weight cannot exceed 80kg"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithWeightExceeding80kg_ShouldThrowInvalidWeightException`

### Scenario: Products cannot have negative weight
```gherkin
Given I want to create a product
When I specify a negative weight
Then the system should reject the product
And display the error "Weight must be greater than zero"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithNegativeWeight_ShouldThrowInvalidWeightException`

### Scenario: Products cannot have zero weight
```gherkin
Given I want to create a product
When I specify a weight of exactly 0kg
Then the system should reject the product
And display the error "Weight must be greater than zero"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithZeroWeight_ShouldThrowInvalidWeightException`

---

## Rule 3: Required Field Validation

### Scenario: Products must have a name
```gherkin
Given I want to create a product
When I provide an empty or null name
Then the system should reject the product
And display the error "Name is required"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithEmptyName_ShouldThrowArgumentException`

### Scenario: Products must have a description
```gherkin
Given I want to create a product
When I provide an empty or null description
Then the system should reject the product
And display the error "Description is required"
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithEmptyDescription_ShouldThrowArgumentException`

---

## Rule 4: Product Boundary Values

### Scenario: Maximum valid product configuration
```gherkin
Given I want to create a product with maximum allowed values
When I specify a price of exactly $10,000 and weight of exactly 80kg
Then the product should be created successfully
And all values should be stored correctly
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithMaximumValidValues_ShouldCreateProduct`

### Scenario: Minimum valid product configuration  
```gherkin
Given I want to create a product with minimum allowed values
When I specify a price of $0.01 and weight of 0.01kg
Then the product should be created successfully
And all values should be stored correctly
```

**Test Implementation:** `CreateProductUseCaseTests.ExecuteAsync_WithMinimumValidValues_ShouldCreateProduct`

---

## Feature: Product Search and Filtering

### Business Context
As a **customer**  
I want to **search and filter products by various criteria**  
So that **I can find products that meet my specific needs**

---

## Rule 5: Price Range Filtering

### Scenario: Filter products by price range
```gherkin
Given there are products with various prices in the system
When I search for products with price between $500 and $1500
Then I should see only products within that price range
And the results should be ordered by price
```

**Test Implementation:** `ProductSpecificationsUnitTests.ByPriceRange_WithValidRange_ShouldReturnProductsInRange`

---

## Rule 6: Weight Range Filtering

### Scenario: Filter products by weight range
```gherkin
Given there are products with various weights in the system
When I search for products with weight between 1kg and 5kg
Then I should see only products within that weight range
```

**Test Implementation:** `ProductSpecificationsUnitTests.ByWeightRange_WithValidRange_ShouldReturnProductsInRange`

---

## Rule 7: Text Search

### Scenario: Search products by name
```gherkin
Given there are products with different names in the system
When I search for products containing "Gaming" in the name
Then I should see all products with "Gaming" in their name
And the search should be case-insensitive
```

**Test Implementation:** `ProductSpecificationsUnitTests.ByNameOrDescription_WithNameSearch_ShouldReturnMatchingProducts`

### Scenario: Search products by description
```gherkin
Given there are products with different descriptions in the system
When I search for products containing "laptop" in the description
Then I should see all products with "laptop" in their description
And the search should be case-insensitive
```

**Test Implementation:** `ProductSpecificationsUnitTests.ByNameOrDescription_WithDescriptionSearch_ShouldReturnMatchingProducts`

---

## Rule 8: Complex Filtering

### Scenario: Affordable and lightweight products
```gherkin
Given there are products with various prices and weights
When I search for affordable products (under $2000) that are lightweight (under 5kg)
Then I should see only products that meet both criteria
```

**Test Implementation:** `ProductSpecificationsUnitTests.AffordableAndLightweight_WithValidCriteria_ShouldReturnMatchingProducts`

---

## Rule 9: Pagination

### Scenario: Paginate product results
```gherkin
Given there are more than 10 products in the system
When I request page 2 with 5 products per page
Then I should receive exactly 5 products
And I should receive pagination metadata (total count, page info)
```

**Test Implementation:** `GetAllProductsUseCaseTests.ExecuteAsync_WithPagination_ShouldReturnPagedResults`

---

## 🔗 Test Coverage Mapping

| Business Rule | Unit Tests | Integration Tests |
|---------------|------------|-------------------|
| **Price Validation** | ✅ 4 tests | ✅ Boundary testing |
| **Weight Validation** | ✅ 4 tests | ✅ Boundary testing |
| **Required Fields** | ✅ 2 tests | ✅ End-to-end validation |
| **Price Filtering** | ✅ Specification tests | ✅ LINQ validation |
| **Weight Filtering** | ✅ Specification tests | ✅ LINQ validation |
| **Text Search** | ✅ Specification tests | ✅ Database search |
| **Complex Filtering** | ✅ Specification tests | ✅ Query composition |
| **Pagination** | ✅ Use case tests | ✅ Database pagination |

---

## 🎯 Business Value

### Risk Mitigation
- **Data Quality**: No products with invalid prices or weights enter the system
- **User Experience**: Clear error messages guide users to correct input
- **Business Compliance**: All products meet business standards

### Operational Benefits
- **Search Efficiency**: Customers can quickly find relevant products
- **Inventory Management**: Products are properly categorized and searchable
- **Scalability**: Pagination ensures good performance with large product catalogs

### Technical Benefits
- **Validation Consistency**: Rules enforced at domain level, not UI level
- **Maintainability**: Business rules clearly documented and tested
- **Reliability**: Comprehensive test coverage prevents regressions