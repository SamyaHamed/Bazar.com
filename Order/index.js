const express = require('express');
const axios = require('axios');
const fs = require('fs').promises;
const path = require('path');

const app = express();
app.use(express.json());

const CATALOG_URL = process.env.CATALOG_URL || 'http://localhost:3001'; // Next server URL (without /api)
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
    // استدعي endpoint الـ decrement في الكاتالوج
    const resp = await axios.post(`${CATALOG_URL}/api/decrement/${id}`);
    if (resp.status === 200 && resp.data && resp.data.ok) {
      const title = resp.data.title || `item-${id}`;

      // سجّل الطلب محلياً في order/data/orders.json
      const orders = await readOrders();
      const newOrder = { id: orders.length + 1, itemId: Number(id), title, time: new Date().toISOString() };
      orders.push(newOrder);
      await writeOrders(orders);

      // اطبع الرسالة المطلوبة
      console.log(`bought book ${title}`);

      return res.status(200).json({ ok: true, order: newOrder });
    } else {
      // لاحظ: الكاتالوج يرجع 400 في حالة نفاد المخزون
      if (resp.data && resp.data.error) {
        return res.status(400).json({ ok: false, error: resp.data.error });
      }
      return res.status(502).json({ ok: false, error: 'catalog_error' });
    }
  } catch (e) {
    // إذا الكاتالوج رجع 400 (out_of_stock) axios سيرمي error يحتوي على response
    if (e.response && e.response.status === 400) {
      const body = e.response.data || {};
      return res.status(400).json({ ok: false, error: body.error || 'out_of_stock' });
    }
    console.error('purchase error', e.message || e);
    return res.status(500).json({ ok: false, error: 'purchase_failed' });
  }
});

app.listen(3002, () => console.log('Order server listening on 3002'));
