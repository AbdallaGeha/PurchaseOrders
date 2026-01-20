# Purchase Orders Management System

.NET 8 Web API managing construction purchase orders (POs) with **accumulative statement processing**, including approvals, advance payments, violation discounts, retention, and partial or full payments with advance deductions.  

Tracks quantities across multiple statements, ensures financial accuracy, and integrates inventory updates upon quantity approval.  

Built with a **service-based architecture, EF Core, DTO-driven endpoints**, and **workflow-enforced state transitions**.  

Designed for real-world procurement where correctness, traceability, and explicit approval flows are critical.

---

## 🎯 Core Goals

- Enforce financial correctness at all times  
- Prevent invalid UI actions via backend validation  
- Track accumulated quantities and amounts precisely  
- Support incremental statements and partial payments  

---

## 🔄 Purchase Order Lifecycle

### States

`Created → Approved → Closed`

### Rules Enforced

**Creation**  
- Purchase orders are created with items, prices, discounts, and advance payment settings  

**Update**  
- Allowed only when state = `Created`  
- Item + Unit duplication is rejected  
- Any duplication throws a `BusinessException`  

**Approval**  
- Allowed only from `Created`  
- Automatically generates an **Advance Payment**  
- Locks the purchase order from further edits  

**Closure**  
- A purchase order can only be closed when:  
Order Total = Paid Amount + Violations Total + Retention Amount


- If not balanced, closure is rejected  
- On close, a **Retention Payment** is automatically generated  

---

## 📄 Statements (Execution Against PO)

### Statement States

`Created → QuantityApproved → Approved`

### CanGetNewStatement Logic

- A new statement is allowed only if:  
  - Purchase Order is `Approved`  
  - At least one item still has remaining quantity  
- Prevents over-execution and duplicate statements  

### New Statement Generation

- Computes next statement number  
- Returns:  
  - Purchase Order header info  
  - All PO items (ordered by LineNo)  
  - Accumulated quantities from previous statements  
  - Financial values (prices, discounts, amounts)  
- DTO is **read-only**, except for **current quantities** entered by the user  

### Statement Creation Rules

- PO must be `Approved`  
- Total statement amount > 0  
- Violations ≤ statement amount  
- For each item: `(Current Quantity + Accumulated Quantity) ≤ PO Quantity`  
- Items with `Quantity = 0` are allowed in UI but ignored in persistence  

### Statement Update Rules

- Updates allowed only when state = `Created`  
- Validation logic similar to creation, ignoring existing statement quantities for accumulation checks  

### Quantity Approval → Inventory Movement

- When state = `QuantityApproved`:  
  - Inventory movements are generated automatically, one per statement item  
  - Inventory updated using **Item, Unit, Store** (from Project)  
  - Ensures inventory matches approved quantities  

### Statement Approval

- Allowed only after quantity approval  
- Once approved, statement becomes eligible for payments  
- Quantities and amounts are locked  

---

## 💰 Payments

- Payments tied to **approved statements** only  

### Payment Types

- **Advance** (auto-generated on PO approval)  
- **Regular**  
- **Retention** (auto-generated on PO close)  

### CanInsertPayment Logic

- Payment allowed only if:  
  - Statement is the last approved statement  
  - Approved statement total > paid amount  
  - Remaining balance > tolerance  

### Insert Payment Rules

- Payment amount > 0  
- Advance deduction < payment amount  
- Payment ≤ allowed remaining amount  
- Advance deduction ≤ allowed advance balance  
- Violations throw a `BusinessException`  

---

## 🏷️ Violations (Discounts / Penalties)

- Stored per statement  
- Optional and editable while statement is in `Created` state  
- Counted in statement validation and PO closure validation  

---

## 📦 Inventory Integration

- Inventory movements created only when statement → `QuantityApproved`  
- Ensures:  
  - No inventory before approval  
  - No duplicate inventory records  
  - Full traceability via `TransactionId`  

---

## 🔍 Lookup Services

- Provides lookup data for UI:  
  - Items  
  - Units (per item)  
  - Currencies  
  - Projects  
  - Suppliers  
- Standardizes dropdown sources and minimizes client-side logic  

---

## 🔐 Validation Strategy (CanX Endpoints)

- Exposes explicit backend validation endpoints:  
  - `CanUpdate`  
  - `CanApprove`  
  - `CanGetNewStatement`  
  - `CanInsertPayment`  
- Benefits:  
  - Keeps UI dumb  
  - Centralizes business rules  
  - No duplicated logic  
  - Easier testing and auditing  

---

## 🛠️ Tech Stack

- **ASP.NET Core Web API (.NET 8)**  
- **Entity Framework Core**  
- **SQL Server** (or compatible RDBMS)  
- **AutoMapper** for DTO mapping  
- DTO design optimized for **Angular frontend integration**  

---

## 🔗 GitHub

Available on GitHub: 🐙 [github.com/abdallageha](https://github.com/abdallageha)

