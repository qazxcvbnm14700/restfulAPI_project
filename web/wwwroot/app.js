// ==========================================================================
// 弦月・本格日式料理屋 - 前端應用邏輯
// Modern Japanese Cuisine Online Ordering System - Application Script
// ==========================================================================

// 全域狀態
const state = {
  menu: {
    restaurantName: '',
    tagline: '',
    categories: [],
    items: []
  },
  activeCategory: '全部料理',
  currentModalDish: null,
  modalSelection: {
    quantity: 1,
    selectedSides: [], // array of side objects: { id, name, price }
    customOptions: {}  // key -> string value
  },
  cart: [],
  lastCreatedOrder: null
};
let dishDetailRequestId = 0;

// DOM 元素快取
const dom = {
  categoryPills: document.getElementById('category-pills'),
  dishesGrid: document.getElementById('dishes-grid'),
  // 購物車
  cartItemsContainer: document.getElementById('cart-items-container'),
  summaryTotalQty: document.getElementById('summary-total-qty'),
  summaryTotalAmount: document.getElementById('summary-total-amount'),
  cartBadge: document.getElementById('cart-count-badge'),
  clearCartBtn: document.getElementById('clear-cart-btn'),
  orderForm: document.getElementById('order-form'),
  submitOrderBtn: document.getElementById('submit-order-btn'),
  // 料理詳細資訊 Modal (核心)
  dishDetailModal: document.getElementById('dish-detail-modal'),
  closeDishModal: document.getElementById('close-dish-modal'),
  modalDishImg: document.getElementById('modal-dish-img'),
  modalDishBadges: document.getElementById('modal-dish-badges'),
  modalDishSubname: document.getElementById('modal-dish-subname'),
  modalDishTitle: document.getElementById('modal-dish-title'),
  modalDishPrice: document.getElementById('modal-dish-price'),
  modalDishDesc: document.getElementById('modal-dish-desc'),
  modalDishIngredients: document.getElementById('modal-dish-ingredients'),
  modalDishOrigin: document.getElementById('modal-dish-origin'),
  modalDishPreptime: document.getElementById('modal-dish-preptime'),
  modalDishCustomOptions: document.getElementById('modal-dish-custom-options'),
  modalDishSidesList: document.getElementById('modal-dish-sides-list'),
  modalQtyMinus: document.getElementById('modal-qty-minus'),
  modalQtyPlus: document.getElementById('modal-qty-plus'),
  modalQtyValue: document.getElementById('modal-qty-value'),
  modalSingleCalc: document.getElementById('modal-single-calc'),
  modalSubtotalCalc: document.getElementById('modal-subtotal-calc'),
  modalAddCartBtn: document.getElementById('modal-add-cart-btn'),
  // 成功確認 Modal
  successModal: document.getElementById('success-modal'),
  modalOrderId: document.getElementById('modal-order-id'),
  modalCustName: document.getElementById('modal-cust-name'),
  modalDiningType: document.getElementById('modal-dining-type'),
  modalTotalAmount: document.getElementById('modal-total-amount'),
  modalCloseBtn: document.getElementById('modal-close-btn'),
  modalQueryBtn: document.getElementById('modal-query-order-btn'),
  // 訂單追蹤 Modal
  trackOrderBtn: document.getElementById('track-order-btn'),
  trackingModal: document.getElementById('tracking-modal'),
  closeTrackingBtn: document.getElementById('close-tracking-btn'),
  searchOrderInput: document.getElementById('search-order-input'),
  doSearchBtn: document.getElementById('do-search-btn'),
  trackingResultBox: document.getElementById('tracking-result-box')
};

// 頁面初始化
document.addEventListener('DOMContentLoaded', () => {
  fetchMenu();
  setupEventListeners();
  setupDiningTypeToggle();
});

// 1. 呼叫 API 載入菜單
async function fetchMenu() {
  try {
    const res = await fetch('/api/menu');
    if (!res.ok) throw new Error(`HTTP error ${res.status}`);
    const data = await res.json();
    state.menu = data;

    renderCategories();
    renderDishes();
  } catch (err) {
    console.error('無法載入菜單:', err);
    dom.dishesGrid.innerHTML = `
      <div class="loading-state" style="color: #ff8577;">
        <span style="font-size: 2.2rem; display: block; margin-bottom: 0.5rem;">⚠️</span>
        <p><strong>菜單載入失敗</strong></p>
        <small>請確認 .NET 後端伺服器是否正常運行中，或稍後再重新整理頁面。</small>
      </div>`;
  }
}

// 渲染類別切換按鈕
function renderCategories() {
  dom.categoryPills.innerHTML = '';
  const categories = state.menu.categories || ['全部料理'];

  categories.forEach(cat => {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = `category-pill ${cat === state.activeCategory ? 'active' : ''}`;
    btn.textContent = cat;
    btn.onclick = () => {
      state.activeCategory = cat;
      document.querySelectorAll('.category-pill').forEach(el => el.classList.remove('active'));
      btn.classList.add('active');
      renderDishes();
    };
    dom.categoryPills.appendChild(btn);
  });
}

// 渲染料理商品卡片列表
function renderDishes() {
  dom.dishesGrid.innerHTML = '';

  const items = state.activeCategory === '全部料理'
    ? state.menu.items
    : state.menu.items.filter(item => item.category === state.activeCategory);

  if (!items || items.length === 0) {
    dom.dishesGrid.innerHTML = `
      <div class="loading-state">
        <p>此分類目前無料理</p>
      </div>`;
    return;
  }

  items.forEach(dish => {
    const card = document.createElement('article');
    card.className = 'dish-card';
    card.setAttribute('aria-label', dish.name);

    card.innerHTML = `
      <div class="dish-card-img-wrap" title="點擊查看 ${escapeHtml(dish.name)} 詳細食材資訊與專屬副餐">
        <img src="${escapeHtml(dish.image)}" alt="${escapeHtml(dish.name)}" class="dish-card-img" loading="lazy">
        <span class="dish-badge-tag">${escapeHtml(dish.badge || '職人手作')}</span>
        <div class="dish-img-overlay">
          <span class="click-hint-badge">🔍 點擊圖片查看料理詳情與副餐</span>
        </div>
      </div>
      <div class="dish-card-body">
        <div class="dish-card-title-row">
          <span class="dish-jp-title">${escapeHtml(dish.subName || '')}</span>
          <h3 class="dish-name-title">${escapeHtml(dish.name)}</h3>
        </div>
        <p class="dish-brief-desc">${escapeHtml(dish.description)}</p>
        <div class="dish-card-footer">
          <div class="dish-card-price">
            <small>NT$</small> ${dish.price}
          </div>
          <button type="button" class="view-detail-btn">
            料理詳情與副餐 🍱
          </button>
        </div>
      </div>
    `;

    // 點擊圖片熱區展開詳細資訊彈窗
    const imgWrap = card.querySelector('.dish-card-img-wrap');
    imgWrap.onclick = () => openDishDetail(dish);

    // 點擊查看按鈕亦可展開
    const detailBtn = card.querySelector('.view-detail-btn');
    detailBtn.onclick = () => openDishDetail(dish);

    dom.dishesGrid.appendChild(card);
  });
}

// 2. 點擊商品時呼叫詳細資訊 API，再顯示專屬副餐 Modal
async function openDishDetail(menuItem) {
  const requestId = ++dishDetailRequestId;

  try {
    const res = await fetch(`/api/menu/${encodeURIComponent(menuItem.id)}`);
    if (!res.ok) throw new Error(`HTTP error ${res.status}`);
    const dish = await res.json();
    if (requestId !== dishDetailRequestId) return;

    renderDishDetail(dish);
  } catch (err) {
    if (requestId !== dishDetailRequestId) return;
    console.error('無法載入料理詳情:', err);
    alert('無法載入料理詳情與專屬副餐，請稍後重試。');
  }
}

function renderDishDetail(dish) {
  state.currentModalDish = dish;

  // 重置選擇狀態
  state.modalSelection = {
    quantity: 1,
    selectedSides: [],
    customOptions: {}
  };

  // 1. 填入料理基本資訊與圖片
  dom.modalDishImg.src = dish.image;
  dom.modalDishImg.alt = dish.name;
  dom.modalDishBadges.innerHTML = `
    <span class="dish-badge-tag">${escapeHtml(dish.badge || '職人推薦')}</span>
    <span class="dish-badge-tag" style="background:#2b2e38; color:#f0cf65; border:1px solid rgba(212,175,55,0.4);">
      ${escapeHtml(dish.category)}
    </span>
  `;
  dom.modalDishSubname.textContent = dish.subName || '';
  dom.modalDishTitle.textContent = dish.name;
  dom.modalDishPrice.textContent = dish.price;
  dom.modalDishDesc.textContent = dish.description;

  // 2. 渲染嚴選食材標籤
  dom.modalDishIngredients.innerHTML = '';
  if (dish.details && dish.details.ingredients) {
    dish.details.ingredients.forEach(ing => {
      const tag = document.createElement('span');
      tag.className = 'spec-tag';
      tag.textContent = ing;
      dom.modalDishIngredients.appendChild(tag);
    });
  }

  // 3. 產地履歷與製作時間
  dom.modalDishOrigin.textContent = dish.details?.origin || '嚴選旬味食材';
  dom.modalDishPreptime.textContent = dish.details?.prepTime || '約 10-15 分鐘';

  // 4. 渲染風味客製化選項 (若該料理有 customOptions)
  dom.modalDishCustomOptions.innerHTML = '';
  if (dish.customOptions && Object.keys(dish.customOptions).length > 0) {
    for (const [key, options] of Object.entries(dish.customOptions)) {
      if (Array.isArray(options) && options.length > 0) {
        // 預設選第一項
        state.modalSelection.customOptions[key] = options[0];

        const groupEl = document.createElement('div');
        groupEl.className = 'custom-option-group';

        const labelText = getOptionLabel(key);
        groupEl.innerHTML = `<label>${escapeHtml(labelText)}</label>`;

        const pillList = document.createElement('div');
        pillList.className = 'custom-pill-list';

        options.forEach((opt, idx) => {
          const pill = document.createElement('button');
          pill.type = 'button';
          pill.className = `custom-pill ${idx === 0 ? 'active' : ''}`;
          pill.textContent = opt;
          pill.onclick = () => {
            pillList.querySelectorAll('.custom-pill').forEach(el => el.classList.remove('active'));
            pill.classList.add('active');
            state.modalSelection.customOptions[key] = opt;
          };
          pillList.appendChild(pill);
        });

        groupEl.appendChild(pillList);
        dom.modalDishCustomOptions.appendChild(groupEl);
      }
    }
  }

  // 5. 核心：渲染每個料理專屬的副餐選擇 (每個商品副餐不同)
  dom.modalDishSidesList.innerHTML = '';
  if (dish.sides && dish.sides.length > 0) {
    dish.sides.forEach(side => {
      const row = document.createElement('div');
      row.className = 'side-item-row';
      row.dataset.sideId = side.id;

      row.innerHTML = `
        <div class="side-item-left">
          <input type="checkbox" class="side-item-checkbox" id="check-${side.id}">
          <div>
            <label for="check-${side.id}" class="side-item-name">${escapeHtml(side.name)}</label>
            <span class="side-item-desc">${escapeHtml(side.description || '')}</span>
          </div>
        </div>
        <div class="side-item-price">+NT$ ${side.price}</div>
      `;

      const checkbox = row.querySelector('.side-item-checkbox');

      // 點擊整列切換勾選
      row.onclick = (e) => {
        if (e.target !== checkbox && e.target.tagName !== 'LABEL') {
          checkbox.checked = !checkbox.checked;
        }
        toggleSideSelection(side, checkbox.checked, row);
      };

      checkbox.onchange = () => {
        toggleSideSelection(side, checkbox.checked, row);
      };

      dom.modalDishSidesList.appendChild(row);
    });
  } else {
    dom.modalDishSidesList.innerHTML = `
      <div style="color: var(--text-muted); font-size: 0.85rem; padding: 0.5rem 0;">
        本道料理暫無額外推薦副餐
      </div>`;
  }

  // 6. 重置數量控制
  dom.modalQtyValue.textContent = '1';

  // 7. 更新金額計算與顯示
  updateModalCalculation();

  // 8. 顯示彈窗
  dom.dishDetailModal.hidden = false;
  document.body.style.overflow = 'hidden';
}

function getOptionLabel(key) {
  const map = {
    riceAmount: '米飯份量客製',
    flavor: '醬汁濃淡調配',
    noodleFirmness: '博多拉麵軟硬度',
    soupRichness: '豚骨湯頭風味濃淡',
    scallion: '青蔥份量調整',
    sauceStyle: '沾醬搭配選擇',
    riceType: '米飯種類選擇',
    wasabi: '山葵搭配方式',
    servingStyle: '食用冷熱方式'
  };
  return map[key] || '客製選項偏好';
}

function toggleSideSelection(side, isSelected, rowEl) {
  const idx = state.modalSelection.selectedSides.findIndex(s => s.id === side.id);
  if (isSelected && idx === -1) {
    state.modalSelection.selectedSides.push(side);
    rowEl.classList.add('selected');
  } else if (!isSelected && idx !== -1) {
    state.modalSelection.selectedSides.splice(idx, 1);
    rowEl.classList.remove('selected');
  }
  updateModalCalculation();
}

function updateModalCalculation() {
  if (!state.currentModalDish) return;

  const basePrice = state.currentModalDish.price;
  const sidesPrice = state.modalSelection.selectedSides.reduce((sum, s) => sum + s.price, 0);
  const singleTotal = basePrice + sidesPrice;
  const subtotal = singleTotal * state.modalSelection.quantity;

  dom.modalSingleCalc.textContent = `NT$ ${singleTotal}`;
  dom.modalSubtotalCalc.textContent = `NT$ ${subtotal}`;
  dom.modalAddCartBtn.textContent = `🍱 將料理與副餐加入點餐清單・NT$ ${subtotal}`;
}

function closeDishDetailModal() {
  dishDetailRequestId++;
  dom.dishDetailModal.hidden = true;
  document.body.style.overflow = '';
  state.currentModalDish = null;
}

// 3. 將料理與副餐打包加入購物車
function addCurrentDishToCart() {
  if (!state.currentModalDish) return;

  const dish = state.currentModalDish;
  const basePrice = dish.price;
  const sidesPrice = state.modalSelection.selectedSides.reduce((sum, s) => sum + s.price, 0);
  const singleTotal = basePrice + sidesPrice;
  const subtotal = singleTotal * state.modalSelection.quantity;

  // 組合客製偏好文字
  const prefParts = Object.values(state.modalSelection.customOptions);
  const prefString = prefParts.length > 0 ? prefParts.join(' / ') : '標準風味';

  const cartItem = {
    cartItemId: Date.now() + Math.random().toString(36).substring(2, 6),
    dishId: dish.id,
    dishName: dish.name,
    dishSubName: dish.subName,
    customPreferences: prefString,
    sides: [...state.modalSelection.selectedSides],
    quantity: state.modalSelection.quantity,
    singlePrice: singleTotal,
    subtotal: subtotal
  };

  state.cart.push(cartItem);
  renderCart();
  closeDishDetailModal();

  // 提供加入成功的視覺提示
  dom.cartBadge.classList.add('pulse');
  setTimeout(() => dom.cartBadge.classList.remove('pulse'), 600);
}

// 4. 渲染購物車內容
function renderCart() {
  const totalQty = state.cart.reduce((sum, it) => sum + it.quantity, 0);
  const totalAmount = state.cart.reduce((sum, it) => sum + it.subtotal, 0);

  dom.cartBadge.textContent = totalQty;
  dom.summaryTotalQty.textContent = `${totalQty} 份`;
  dom.summaryTotalAmount.textContent = `NT$ ${totalAmount}`;

  if (state.cart.length === 0) {
    dom.cartItemsContainer.innerHTML = `
      <div class="empty-cart-msg">
        <span class="empty-icon">🍣</span>
        <p>點餐清單目前尚無餐點</p>
        <small>點擊左側餐點圖片查看詳情，挑選專屬副餐加入清單吧！</small>
      </div>`;
    dom.submitOrderBtn.disabled = true;
    return;
  }

  dom.submitOrderBtn.disabled = false;
  dom.cartItemsContainer.innerHTML = '';

  state.cart.forEach((item, idx) => {
    const row = document.createElement('div');
    row.className = 'cart-item-row';

    const sidesDesc = item.sides && item.sides.length > 0
      ? `副餐：${item.sides.map(s => `${escapeHtml(s.name)} (+NT$${s.price})`).join('、')}`
      : '無加選副餐';

    row.innerHTML = `
      <div class="cart-item-info">
        <div class="cart-item-title">${escapeHtml(item.dishName)} × ${item.quantity}</div>
        <div class="cart-item-pref">${escapeHtml(item.customPreferences)}</div>
        <div class="cart-item-sides">${sidesDesc}</div>
      </div>
      <div class="cart-item-right">
        <div class="cart-item-price">NT$ ${item.subtotal}</div>
        <button type="button" class="cart-remove-btn" onclick="removeFromCart(${idx})">移除</button>
      </div>
    `;

    dom.cartItemsContainer.appendChild(row);
  });
}

window.removeFromCart = function(index) {
  state.cart.splice(index, 1);
  renderCart();
};

// 5. 呼叫 2號 API 送出日式料理訂單
async function submitOrder(e) {
  e.preventDefault();

  if (state.cart.length === 0) {
    alert('點餐清單沒有任何料理，無法送出訂單！');
    return;
  }

  const custName = document.getElementById('cust-name').value.trim();
  const phone = document.getElementById('cust-phone').value.trim();
  const diningType = document.querySelector('input[name="diningType"]:checked')?.value || '內用';
  const tableNo = document.getElementById('table-no').value.trim();
  const address = document.getElementById('delivery-address').value.trim();
  const note = document.getElementById('cust-note').value.trim();

  if (!custName || !phone) {
    alert('請填寫顧客姓名與連絡電話！');
    return;
  }

  if (diningType === '內用' && !tableNo) {
    alert('內用訂單請填寫桌號！');
    document.getElementById('table-no').focus();
    return;
  }

  if (diningType === '外送' && !address) {
    alert('外送訂單請填寫地址！');
    document.getElementById('delivery-address').focus();
    return;
  }

  const totalAmount = state.cart.reduce((sum, it) => sum + it.subtotal, 0);

  const payload = {
    customerName: custName,
    phone: phone,
    diningType: diningType,
    tableNo: tableNo,
    address: address,
    note: note,
    items: state.cart.map(it => ({
      dishId: it.dishId,
      dishName: it.dishName,
      customPreferences: it.customPreferences,
      sides: it.sides.map(s => ({
        id: s.id,
        name: s.name,
        price: s.price
      })),
      quantity: it.quantity,
      subtotal: it.subtotal
    })),
    totalAmount: totalAmount
  };

  dom.submitOrderBtn.disabled = true;
  const originalText = dom.submitOrderBtn.innerHTML;
  dom.submitOrderBtn.innerHTML = `
    <span class="btn-text">料理長接單製作中...</span>
    <span class="btn-sub">正在傳送您的日式料理訂單</span>
  `;

  try {
    const res = await fetch('/api/orders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();

    if (data.success) {
      state.lastCreatedOrder = data.orderId;
      showSuccessModal(data.orderId, custName, diningType, totalAmount);
      // 清空購物車與表單
      state.cart = [];
      renderCart();
      dom.orderForm.reset();
      setupDiningTypeToggle();
    } else {
      alert('建立訂單失敗：' + (data.message || '請稍候重試'));
    }
  } catch (err) {
    console.error('送出訂單錯誤:', err);
    alert('送出訂單發生錯誤，請檢查網路連線或伺服器運行狀態。');
  } finally {
    dom.submitOrderBtn.disabled = false;
    dom.submitOrderBtn.innerHTML = originalText;
  }
}

// 6. 呼叫 3號 API 查詢日式料理訂單進度
async function queryOrder(orderId) {
  if (!orderId) {
    alert('請輸入要查詢的訂單編號');
    return;
  }

  dom.trackingResultBox.hidden = false;
  dom.trackingResultBox.innerHTML = `
    <div style="text-align:center; padding: 1.5rem; color: var(--text-muted);">
      <div class="spinner" style="width:28px; height:28px;"></div>
      <p style="font-size:0.88rem;">正在向料理長查詢訂單狀態...</p>
    </div>
  `;

  try {
    const res = await fetch(`/api/orders/${encodeURIComponent(orderId)}`);
    if (res.status === 404) {
      dom.trackingResultBox.innerHTML = `
        <div style="color: #ff6b6b; text-align: center; padding: 1rem;">
          <p>❌ 查無此訂單編號：<strong>${escapeHtml(orderId)}</strong></p>
          <small style="color: var(--text-muted); display:block; margin-top:0.3rem;">請核對訂單編號格式是否正確（例：ORD-YYYYMMDD-001）</small>
        </div>`;
      return;
    }
    if (!res.ok) throw new Error(`HTTP ${res.status}`);

    const data = await res.json();

    const itemsHtml = (data.items || []).map(it => {
      const sidesStr = it.sides && it.sides.length > 0
        ? `<div style="color: var(--accent-gold); font-size: 0.78rem; padding-left: 0.8rem; margin-top: 0.15rem;">
             ＋ 副餐：${escapeHtml(it.sides.join('、'))}
           </div>`
        : '';

      return `
        <div style="padding: 0.6rem 0; border-bottom: 1px dashed var(--border-subtle);">
          <div style="display:flex; justify-content:space-between; align-items:baseline;">
            <strong style="color: #fff; font-size: 0.92rem;">${escapeHtml(it.dishName)} × ${it.quantity}</strong>
            <span style="color: var(--accent-gold); font-weight:700;">NT$ ${it.subtotal}</span>
          </div>
          <div style="font-size: 0.78rem; color: var(--text-muted); margin-top: 0.15rem;">
            風味客製：${escapeHtml(it.customPreferences || '標準風味')}
          </div>
          ${sidesStr}
        </div>
      `;
    }).join('');

    dom.trackingResultBox.innerHTML = `
      <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom: 0.8rem;">
        <div>
          <span style="font-size: 0.72rem; color: var(--text-muted); display:block;">訂單編號</span>
          <strong style="font-family: monospace; font-size: 1.1rem; color: var(--accent-gold);">${escapeHtml(data.orderId)}</strong>
        </div>
        <span class="tracking-status-badge">⚡ ${escapeHtml(data.status)}</span>
      </div>

      <div style="font-size: 0.82rem; color: var(--text-secondary); line-height: 1.6; margin-bottom: 0.8rem; background: rgba(255,255,255,0.03); padding: 0.6rem 0.8rem; border-radius: 6px;">
        <div>顧客姓名：<strong>${escapeHtml(data.customerName)}</strong></div>
        <div>用餐方式：<strong>${escapeHtml(data.diningType)}</strong> ${data.tableNo && data.tableNo !== '無' ? `(${escapeHtml(data.tableNo)})` : ''}</div>
        ${data.address ? `<div>外送地址：<strong>${escapeHtml(data.address)}</strong></div>` : ''}
        ${data.note ? `<div>顧客備註：${escapeHtml(data.note)}</div>` : ''}
        <div style="color: var(--text-muted); font-size: 0.72rem; margin-top: 0.2rem;">下單時間：${new Date(data.createdAt).toLocaleString('zh-TW')}</div>
      </div>

      <div style="margin: 0.8rem 0;">
        <span style="font-size: 0.78rem; font-weight:700; color: var(--text-secondary);">料理明細：</span>
        ${itemsHtml}
      </div>

      <div style="display:flex; justify-content:space-between; align-items:baseline; margin-top: 0.8rem; padding-top: 0.6rem; border-top: 1px solid var(--border-subtle);">
        <strong style="color: #fff;">結帳總額</strong>
        <span style="color: var(--accent-gold); font-size: 1.3rem; font-weight: 800;">NT$ ${data.totalAmount}</span>
      </div>
    `;
  } catch (err) {
    console.error('查詢失敗:', err);
    dom.trackingResultBox.innerHTML = '<p style="color:#ff6b6b; text-align:center;">查詢訂單發生錯誤，請稍候重試。</p>';
  }
}

// 7. 彈窗顯示輔助函式
function showSuccessModal(orderId, name, diningType, amount) {
  dom.modalOrderId.textContent = orderId;
  dom.modalCustName.textContent = name;
  dom.modalDiningType.textContent = diningType;
  dom.modalTotalAmount.textContent = `NT$ ${amount}`;
  dom.successModal.hidden = false;
}

function setupDiningTypeToggle() {
  const tableGroup = document.getElementById('table-no-group');
  const tableInput = document.getElementById('table-no');
  const addressGroup = document.getElementById('delivery-address-group');
  const addressInput = document.getElementById('delivery-address');
  const radios = document.querySelectorAll('input[name="diningType"]');

  const updateDiningFields = () => {
    const diningType = document.querySelector('input[name="diningType"]:checked')?.value;
    const isDineIn = diningType === '內用';
    const isDelivery = diningType === '外送';

    tableGroup.hidden = !isDineIn;
    tableInput.disabled = !isDineIn;
    tableInput.required = isDineIn;
    addressGroup.hidden = !isDelivery;
    addressInput.disabled = !isDelivery;
    addressInput.required = isDelivery;
  };

  radios.forEach(r => {
    r.onchange = updateDiningFields;
  });
  updateDiningFields();
}

// 事件監聽綁定
function setupEventListeners() {
  // Modal 數量增減
  dom.modalQtyMinus.onclick = () => {
    if (state.modalSelection.quantity > 1) {
      state.modalSelection.quantity--;
      dom.modalQtyValue.textContent = state.modalSelection.quantity;
      updateModalCalculation();
    }
  };

  dom.modalQtyPlus.onclick = () => {
    state.modalSelection.quantity++;
    dom.modalQtyValue.textContent = state.modalSelection.quantity;
    updateModalCalculation();
  };

  // 加入購物車
  dom.modalAddCartBtn.onclick = addCurrentDishToCart;

  // 關閉料理詳情 Modal
  dom.closeDishModal.onclick = closeDishDetailModal;
  dom.dishDetailModal.onclick = (e) => {
    if (e.target === dom.dishDetailModal) {
      closeDishDetailModal();
    }
  };

  // 清空購物車
  dom.clearCartBtn.onclick = () => {
    if (state.cart.length > 0 && confirm('確定要清空點餐清單嗎？')) {
      state.cart = [];
      renderCart();
    }
  };

  // 表單送出
  dom.orderForm.onsubmit = submitOrder;

  // 下單成功 Modal 按鈕
  dom.modalCloseBtn.onclick = () => {
    dom.successModal.hidden = true;
  };
  dom.modalQueryBtn.onclick = () => {
    dom.successModal.hidden = true;
    dom.trackingModal.hidden = false;
    dom.searchOrderInput.value = state.lastCreatedOrder || '';
    if (state.lastCreatedOrder) {
      queryOrder(state.lastCreatedOrder);
    }
  };

  // 訂單追蹤 Modal 按鈕
  dom.trackOrderBtn.onclick = () => {
    dom.trackingModal.hidden = false;
  };
  dom.closeTrackingBtn.onclick = () => {
    dom.trackingModal.hidden = true;
  };
  dom.trackingModal.onclick = (e) => {
    if (e.target === dom.trackingModal) {
      dom.trackingModal.hidden = true;
    }
  };
  dom.doSearchBtn.onclick = () => {
    queryOrder(dom.searchOrderInput.value.trim());
  };
  dom.searchOrderInput.onkeydown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      queryOrder(dom.searchOrderInput.value.trim());
    }
  };

  // 按下 ESC 鍵關閉彈窗
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      if (!dom.dishDetailModal.hidden) closeDishDetailModal();
      if (!dom.trackingModal.hidden) dom.trackingModal.hidden = true;
      if (!dom.successModal.hidden) dom.successModal.hidden = true;
    }
  });
}

function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
