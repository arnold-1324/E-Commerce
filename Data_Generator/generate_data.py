import json
import uuid
from faker import Faker
import random
from datetime import datetime, timedelta

fake = Faker()

# ========================
# COMMON DATA DEFINITIONS
# ========================
product_categories = {
    "Electronics": {
        "subcategories": ["Laptops", "Smartphones", "Tablets", "Smartwatches", "Gaming Consoles"],
        "brands": ["Apple", "Samsung", "Dell", "Sony", "Microsoft"],
        "attributes": ["Storage Capacity", "Screen Size", "Battery Life", "Processor Type"]
    },
    "Clothing": {
        "subcategories": ["Men's Wear", "Women's Wear", "Kids", "Sportswear", "Accessories"],
        "brands": ["Nike", "Zara", "Levi's", "H&M", "Adidas"],
        "attributes": ["Size", "Color", "Material", "Season"]
    },
    "Home & Kitchen": {
        "subcategories": ["Cookware", "Small Appliances", "Furniture", "Storage", "Decor"],
        "brands": ["KitchenAid", "IKEA", "OXO", "Cuisinart", "Pyrex"],
        "attributes": ["Dimensions", "Capacity", "Power Source", "Warranty"]
    }
}

search_keywords = {
    "Electronics": ["fast", "powerful", "portable", "high-resolution", "wireless"],
    "Clothing": ["comfortable", "breathable", "stylish", "durable", "trendy"],
    "Home & Kitchen": ["easy clean", "space-saving", "energy efficient", "non-stick", "ergonomic"]
}

# ========================
# DATA GENERATION LOGIC
# ========================
def generate_product():
    category = random.choice(list(product_categories.keys()))
    subcategory = random.choice(product_categories[category]["subcategories"])
    brand = random.choice(product_categories[category]["brands"])
    
    # Generate semantically related tags
    base_tags = [
        f"Brand:{brand}",
        f"Category:{category}",
        f"Subcategory:{subcategory}"
    ]
    
    # Add search-friendly attributes
    attributes = random.sample(product_categories[category]["attributes"], 2)
    for attr in attributes:
        base_tags.append(f"{attr}:{fake.word()}")
    
    # Add recommendation signals
    if category == "Electronics":
        base_tags.append(f"CompatibleWith:{random.choice(['iPhone', 'Android', 'Windows', 'MacOS'])}")
    elif category == "Clothing":
        base_tags.append(f"Occasion:{random.choice(['Casual', 'Formal', 'Sports', 'Party'])}")
    
    return {
        "product_id": str(uuid.uuid4()),
        "name": f"{brand} {fake.word().capitalize()} {subcategory}",
        "description": generate_meaningful_description(category, subcategory, brand),
        "price": generate_realistic_price(category),
        "category": category,
        "subcategory": subcategory,
        "attributes": {attr: fake.word() for attr in attributes},
        "stock": random.randint(0, 500),
        "brand": brand,
        "rating": round(random.uniform(3.5, 5.0), 1),
        "tags": base_tags + random.sample(search_keywords[category], 2),
        "related_products": [],
        "image_url": f"https://source.unsplash.com/200x200/?{category.lower().replace(' ','')},{subcategory.lower().replace(' ','')}"
    }

def generate_meaningful_description(category, subcategory, brand):
    descriptors = {
        "Electronics": [
            f"{brand}'s latest {subcategory} featuring {fake.word()} technology",
            f"Professional-grade {subcategory} with {random.choice(['enhanced performance', 'long battery life', '4K display'])}"
        ],
        "Clothing": [
            f"{brand}'s premium {subcategory} made with {fake.word()} fabric",
            f"Trend-setting {subcategory} designed for {random.choice(['all-day comfort', 'athletic performance', 'formal occasions'])}"
        ]
    }
    return random.choice(descriptors.get(category, ["High-quality product designed for modern needs"]))

def generate_realistic_price(category):
    price_ranges = {
        "Electronics": (299.99, 2999.99),
        "Clothing": (19.99, 199.99),
        "Home & Kitchen": (49.99, 499.99)
    }
    return round(random.uniform(*price_ranges[category]), 2)

# ========================
# RELATED SERVICE DATA
# ========================
def generate_user():
    return {
        "user_id": str(uuid.uuid4()),
        "name": fake.name(),
        "email": fake.email(),
        "purchase_history": [],
        "search_history": [],
        "preferences": random.sample(list(product_categories.keys()), 2)
    }

def generate_order(products, users):
    product = random.choice(products)
    user = random.choice(users)
    
    return {
        "order_id": str(uuid.uuid4()),
        "user_id": user["user_id"],
        "products": [{
            "product_id": product["product_id"],
            "quantity": random.randint(1, 3)
        }],
        "timestamp": str(datetime.now() - timedelta(days=random.randint(1, 365)))
    }

# ========================
# MAIN GENERATION
# ========================
if __name__ == "__main__":
    # Generate core datasets
    products = [generate_product() for _ in range(500)]
    users = [generate_user() for _ in range(100)]
    orders = [generate_order(products, users) for _ in range(1000)]

    # Create relationships for recommendation service
    for product in products:
        related = random.sample(
            [p for p in products if p["category"] == product["category"] and p["product_id"] != product["product_id"]], 
            3
        )
        product["related_products"] = [p["product_id"] for p in related]

    # Save datasets
    with open('products.json', 'w') as f:
        json.dump(products, f, indent=2)
    
    with open('users.json', 'w') as f:
        json.dump(users, f, indent=2)
    
    with open('orders.json', 'w') as f:
        json.dump(orders, f, indent=2)

    print(f"Generated:\n- {len(products)} products\n- {len(users)} users\n- {len(orders)} orders")