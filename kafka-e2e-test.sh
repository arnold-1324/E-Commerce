#!/usr/bin/env bash
set -euo pipefail

BASE_URL="http://localhost:8000"
PRODUCT_NAME="Postman Widget Test"
MAX_RETRIES=10
SLEEP_SECONDS=2

# Step 1: Create product
echo "📦 Creating product..."
CREATE_RES=$(curl -s -X POST "$BASE_URL/api/product" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "'"$PRODUCT_NAME"'",
    "description": "A widget created for curl-based e2e test",
    "price": 9.99,
    "category": "Test",
    "subcategory": "Widget",
    "attributes": { "Color": "Red", "Size": "M" },
    "stock": 100,
    "brand": "TestBrand",
    "rating": 4.2,
    "tags": ["test", "widget"],
    "related_products": [],
    "image_url": "https://example.com/widget.png"
  }')

PRODUCT_ID=$(echo "$CREATE_RES" | jq -r '.productId')

if [[ "$PRODUCT_ID" == "null" || -z "$PRODUCT_ID" ]]; then
  echo "❌ Failed to create product"
  exit 1
fi

echo "✅ Product created with ID: $PRODUCT_ID"

# Step 2: Poll search endpoint
echo "🔍 Polling /api/search/query to find the product..."
for ((i = 1; i <= MAX_RETRIES; i++)); do
  echo "⏳ Attempt $i..."

  SEARCH_RES=$(curl -s "$BASE_URL/api/search/query?q=$(echo $PRODUCT_NAME | jq -sRr @uri)&page=1&size=20")
  FOUND=$(echo "$SEARCH_RES" | jq --arg pid "$PRODUCT_ID" '.items[]? | select(.productId == $pid)' || true)

  if [[ -n "$FOUND" ]]; then
    echo "✅ Product indexed in Elasticsearch!"
    break
  fi

  if [[ $i -eq $MAX_RETRIES ]]; then
    echo "❌ Product not indexed after $MAX_RETRIES attempts"
    exit 1
  fi

  sleep $SLEEP_SECONDS
done

# Step 3: Cleanup
echo "🧹 Deleting product with ID: $PRODUCT_ID..."
curl -s -X DELETE "$BASE_URL/api/product/$PRODUCT_ID" > /dev/null
echo "✅ Product deleted."

echo "🎉 Kafka → ES → Search E2E test passed!"
