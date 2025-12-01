import requests

base_url = "http://127.0.0.1:8000"

print("Testing API...")

r = requests.get(f"{base_url}/health")
print(f"Health: {r.json()}")

r = requests.post(f"{base_url}/predict", json={"text": "I love this!"})
print(f"Prediction: {r.json()}")
