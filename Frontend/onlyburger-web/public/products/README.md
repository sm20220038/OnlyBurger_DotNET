# Product images

Drop product photos in this folder to have them show on the menu and cart.

## Naming

Name each file after the product id, as a `.jpg`:

```
public/products/1.jpg   -> product with id 1 (Classic Burger)
public/products/2.jpg   -> product with id 2 (Cheeseburger)
...
```

Find a product's id on the Manage Menu page (admin) or in the API response from
`GET /api/products`.

Until an image exists for a product, the menu shows a clean placeholder with the
product name. No code changes are needed when you add images.

## Changing the convention

The path is built in one place: `productImageUrl(productId)` in
`src/utils/format.js`. Edit it there if you want a different folder, extension, or
naming scheme (for example, using the product name instead of the id).
