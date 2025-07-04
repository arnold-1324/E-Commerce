import json

def extract_product_info(file_path):
    """
    Extracts product_id and name from a JSON file containing product data.
    
    Args:
        file_path (str): Path to the input JSON file
        
    Returns:
        list: List of tuples containing (product_id, name)
    """
    try:
        with open(file_path, 'r') as file:
            products = json.load(file)
            
        product_info = [(product.get('product_id'), product.get('name')) for product in products]
        return product_info
    
    except FileNotFoundError:
        print(f"Error: File not found at {file_path}")
        return []
    except json.JSONDecodeError:
        print("Error: Invalid JSON format in the input file")
        return []
    except Exception as e:
        print(f"An unexpected error occurred: {str(e)}")
        return []

def save_to_json(data, output_file):
    """
    Saves data to a JSON file.
    
    Args:
        data: Data to be saved
        output_file (str): Path to the output JSON file
    """
    try:
        # Convert list of tuples to list of dictionaries for better JSON structure
        formatted_data = [{"product_id": pid, "name": name} for pid, name in data]
        
        with open(output_file, 'w') as file:
            json.dump(formatted_data, file, indent=4)
        print(f"Data successfully saved to {output_file}")
    except Exception as e:
        print(f"Error saving to JSON file: {str(e)}")

# File paths
input_file = "/workspace/E-Commerce/Data_Generator/products.json"
output_file = "/workspace/E-Commerce/Data_Generator/product_ids_and_names.json"

# Extract product info
product_info = extract_product_info(input_file)

if product_info:
    print("Extracted Product Info (ID, Name):")
    for pid, name in product_info:
        print(f"{pid}: {name}")
    
    # Save to new JSON file
    save_to_json(product_info, output_file)
else:
    print("No product information was extracted.")