from flask import Flask, request, jsonify
import os
import requests
import logging

app = Flask(__name__)
logging.basicConfig(level=logging.INFO)

CATALOG_URL = os.environ.get('CATALOG_URL', 'http://localhost:5142')  # Catalog
ORDER_URL   = os.environ.get('ORDER_URL',   'http://localhost:3002')  # Order

def forward_get(url, timeout=5):
    return requests.get(url, timeout=timeout)

def forward_post(url, json=None, timeout=8):
    return requests.post(url, json=json, timeout=timeout)

def forward_put(url, json=None, timeout=8):
    return requests.put(url, json=json, timeout=timeout)

@app.route('/search', methods=['GET'])
def search():
    topic = request.args.get('topic', '')
    logging.info(f"Front: search topic='{topic}'")
    if not topic:
        return jsonify({"error": "missing topic parameter"}), 400
    try:
        encoded = requests.utils.requote_uri(topic)
        resp = forward_get(f"{CATALOG_URL}/api/catalog/book/search/{encoded}")
        return (resp.content, resp.status_code, {'Content-Type': 'application/json'})
    except requests.RequestException as e:
        logging.error("Catalog unreachable: %s", e)
        return jsonify({"error": "catalog_unreachable"}), 502

@app.route('/info/<int:item_id>', methods=['GET'])
def info(item_id):
    logging.info(f"Front: info id={item_id}")
    try:
        resp = forward_get(f"{CATALOG_URL}/api/catalog/book/info/{item_id}")
        return (resp.content, resp.status_code, {'Content-Type': 'application/json'})
    except requests.RequestException as e:
        logging.error("Catalog unreachable: %s", e)
        return jsonify({"error": "catalog_unreachable"}), 502

@app.route('/purchase/<int:item_id>', methods=['POST'])
def purchase(item_id):
    logging.info(f"Front: purchase id={item_id}")
    try:
        resp = forward_post(f"{ORDER_URL}/purchase/{item_id}")
        return (resp.content, resp.status_code, {'Content-Type': 'application/json'})
    except requests.RequestException as e:
        logging.error("Order unreachable: %s", e)
        return jsonify({"error": "order_unreachable"}), 502

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=3000, debug=False)
