Purchase Orders Management System

.NET 8 Web API managing construction POs with accumulative statement processing, including approvals, advance payments, violation discounts, retention, and partial or full payments with advance deductions

Tracks quantities across multiple statements, ensures financial accuracy, and integrates inventory updates upon quantity approval

Service-based architecture with EF Core, DTO-driven endpoints, and workflow-enforced state transitions

This system is designed for real-world procurement, where correctness, traceability, and explicit approval flows are critical.

🎯 Core Goals

Enforce financial correctness at all times

Prevent invalid UI actions via backend validation

Track accumulated quantities and amounts precisely

Support incremental statements and partial payments

🔄 Purchase Order Lifecycle
States
Created → Approved → Closed

Rules Enforced
Creation

Purchase orders are created with items, prices, discounts, and advance payment settings

Update

Allowed only when state = Created

Item + Unit duplication is rejected

Any duplication throws a BusinessException

ItemId + UnitId must be unique per order

Approval

Allowed only from Created

Automatically creates an Advance Payment

Locks the purchase order from further edits

Closure

A purchase order can only be closed when:

Order Total
=
Paid Amount
+ Violations Total
+ Retention Amount


If not balanced, closure is rejected.

On close:

A Retention Payment is automatically generated

📄 Statements (Execution Against PO)

Statements represent incremental execution of a purchase order.

Statement States
Created → QuantityApproved → Approved

CanGetNewStatement Logic

A new statement is allowed only if:

Purchase Order is Approved

At least one item still has remaining quantity

This prevents over-execution and duplicate statements.

New Statement Generation

GetNewStatement:

Computes next statement number

Returns:

Purchase Order header info

All PO items (ordered by LineNo)

Accumulated quantities from previous statements

Financial values (prices, discounts, amounts)

This DTO is read-only, except for current quantities entered by the user.

Statement Creation Rules

A statement is valid only if:

Purchase Order is Approved

Total statement amount > 0

Violations ≤ statement amount

For each item:

(Current Quantity + Accumulated Quantity) ≤ PO Quantity


Items with Quantity = 0 are allowed in the UI but ignored during persistence.

Statement Update Rules

Updates are allowed only when state = Created

Validation logic is identical to creation, except:

Existing statement quantities are excluded from accumulation checks

Quantity Approval → Inventory Movement

When QuantityApproved:

Inventory movements are generated automatically

One movement per statement item

Inventory is updated using:

Item

Unit

Store (derived from Project)

This ensures:

Inventory always matches approved quantities

Statement Approval

Allowed only after quantity approval

Once approved:

Statement becomes eligible for payments

Quantities and amounts are locked

💰 Payments

Payments are always tied to approved statements.

Payment Types

Advance (auto-generated on PO approval)

Regular

Retention (auto-generated on PO close)

CanInsertPayment Logic

A payment is allowed only if:

The statement is the last approved statement

Approved statement total > paid amount

Remaining balance > tolerance

This guarantees:

No payments against outdated or partial statements

Insert Payment Rules

Before inserting a payment:

Payment amount > 0

Advance deduction < payment amount

Payment ≤ allowed remaining amount

Advance deduction ≤ allowed advance balance

Any violation results in a BusinessException.

🏷️ Violations (Discounts / Penalties)

Stored per statement

Optional

Editable while statement is in Created state

Counted in:

Statement validation

Purchase order closure validation

📦 Inventory Integration

Inventory movements are created only when:

Statement → QuantityApproved


This ensures:

No inventory before approval

No duplicate inventory records

Full traceability via TransactionId

🔍 Lookup Services

Provides lookup data for UI:

Items

Units (per item)

Currencies

Projects

Suppliers

Designed to:

Minimize client logic

Standardize dropdown sources

🔐 Validation Strategy (CanX Endpoints)

The backend exposes explicit validation endpoints:

CanUpdate

CanApprove

CanGetNewStatement

CanInsertPayment

Why?

UI stays dumb

Business rules stay centralized

No duplicated logic

Easier testing and auditing

🛠️ Tech Stack

ASP.NET Core Web API

Entity Framework Core

SQL Server (or compatible RDBMS)

AutoMapper

Angular-friendly DTO design


