from fastapi import FastAPI
from pydantic import BaseModel
import random
import time

app = FastAPI()


class InferenceRequest(BaseModel):
    text: str


@app.get("/health")
def health():
    return {"status": "ok"}

# mock inference model endpoint
@app.post("/predict")
def predict(request: InferenceRequest):
    time.sleep(0.1)
    
    sentiments = ["positive", "negative", "neutral"]
    result = random.choice(sentiments)
    confidence = round(random.uniform(0.7, 0.99), 2)
    
    return {
        "text": request.text,
        "sentiment": result,
        "confidence": confidence
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)
