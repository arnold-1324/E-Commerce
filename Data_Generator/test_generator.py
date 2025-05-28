import json
import uuid
import pytest

def test_generated_files():
    # Test products.json
    with open('products.json') as f:
        products = json.load(f)
        assert len(products) >= 500
        assert all('product_id' in p for p in products)
        assert all(uuid.UUID(p['product_id']) for p in products[:10])

    # Test users.json
    with open('users.json') as f:
        users = json.load(f)
        assert len(users) >= 100
        assert all('user_id' in u for u in users)

    # Test orders.json
    with open('orders.json') as f:
        orders = json.load(f)
        assert len(orders) >= 1000

if __name__ == "__main__":
    pytest.main(["-v"])