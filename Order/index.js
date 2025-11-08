const express = require('express');
const axios = require('axios');
const fs = require('fs').promises;
const path = require('path');

const app = express();
app.use(express.json());

const CATALOG_URL = process.env.CATALOG_URL || 'http://localhost:5142'; // Catalog 
const ORDERS_FILE = path.join(__dirname, 'data', 'orders.json');

async function readOrders() {
  try {
    const txt = await fs.readFile(ORDERS_FILE, 'utf8');
    return JSON.parse(txt);
  } catch {
    return [];
  }
}
async function writeOrders(arr) {
  await fs.mkdir(path.join(__dirname, 'data'), { recursive: true });
  await fs.writeFile(ORDERS_FILE, JSON.stringify(arr, null, 2));
}

app.post('/purchase/:id', async (req, res) => {
  const id = req.params.id;
  try {
    const body = { quantityDelta: -1, QuantityDelta: -1 };

    const resp = await axios.put(`${CATALOG_URL}/api/catalog/book/${id}`, body, {
      headers: { 'Content-Type': 'application/json' },
      timeout: 8000
    });

    if (resp.status === 200 && resp.data) {
      const title = resp.data.title || resp.data.Title || `item-${id}`;


      const orders = await readOrders();
      const newOrder = { id: orders.length + 1, itemId: Number(id), title, time: new Date().toISOString() };
      orders.push(newOrder);
      await writeOrders(orders);

      console.log(`bought book ${title}`);
      return res.status(200).json({ ok: true, order: newOrder });
    } else {
      return res.status(502).json({ ok: false, error: 'catalog_error' });
    }
  } catch (e) {
    if (e.response) {
      const status = e.response.status;
      const data = e.response.data || {};

      if (status === 400) {
        const msg = data.message || data.error || 'out_of_stock';
        return res.status(400).json({ ok: false, error: msg });
      }
      if (status === 404) {
        const msg = data.message || 'not_found';
        return res.status(404).json({ ok: false, error: msg });
      }
      if (status === 409) {
        const msg = data.message || 'conflict';
        return res.status(409).json({ ok: false, error: msg });
      }
      return res.status(502).json({ ok: false, error: 'catalog_error', details: data });
    }

    console.error('purchase error', e.message || e);
    return res.status(500).json({ ok: false, error: 'purchase_failed' });
  }
});

app.listen(3002, () => console.log('Order server listening on 3002'));
