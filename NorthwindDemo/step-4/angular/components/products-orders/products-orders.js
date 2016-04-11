angular.module('app.products', [])
    .controller('ProductsOrdersController', ['$q', 'Orders', 'OrderInformation', 'ProductDetails', 'ProductSales', function ($q, Orders, OrderInformation, ProductDetails, ProductSales) {

        var employees = [
            { value: 1, text: "Nancy Davolio" },
            { value: 2, text: "Andrew Fuller" },
            { value: 3, text: "Janet Leverling" },
            { value: 4, text: "Margaret Peacock" },
            { value: 5, text: "Steven Buchanan" },
            { value: 6, text: "Michael Suyama" },
            { value: 7, text: "Robert King" },
            { value: 8, text: "Laura Callahan" },
            { value: 9, text: "Anne Dodsworth" }
        ];

        var shippers = [
            { value: 1, text: "Speedy Express" },
            { value: 2, text: "United Package" },
            { value: 3, text: "Federal Shipping" }
        ];

        this.ordersDataSource = new kendo.data.DataSource({
            pageSize: 20
        });

        this.orders = Orders.query();
        this.orderDetails = OrderInformation.query();
        this.productDetails = ProductDetails.query();
        this.productSales = ProductSales.query();

        $q.all([this.orders.$promise, this.orderDetails.$promise, this.productDetails.$promise, this.productSales.$promise]).then(function() {
            this.orders.forEach(function(order) {
                order.OrderDate = kendo.parseDate(order.OrderDate);
            });
            this.ordersDataSource.data(this.orders);

            this.productsColumns = [
                {
                    field: "ProductID",
                    title: "PRODUCT NAME",
                    values: this.productDetails.map(function(product) {
                        return {
                            value: product.ProductID,
                            text: product.ProductName
                        };
                    })
                },
                { field: "UnitPrice", title: "UNIT PRICE", format: "{0:c}", width: 220 },
                { field: "Quantity", title: "QUANTITY", width: 220 },
                { field: "Discount", title: "DISCOUNT", format: "{0:p}", width: 200, footerTemplate: "Grand total:" },
                { field: "Total", title: "TOTAL", format: "{0:c}", footerTemplate: "<span name='sum'>#=kendo.toString(sum, 'c')#</span>", width: 120 }
            ];
        }.bind(this));


        this.product = function(productId) {
            return this.productDetails.filter(function(product) {
                return product.ProductID === productId;
            })[0];
        };

        var productSalesDataSources = {};

        this.productSalesDataSource = function(productId) {
            var productSalesDataSource = productSalesDataSources[productId];

            if (!productSalesDataSource) {
                productSalesDataSource = new kendo.data.DataSource({
                    data: this.productSales.filter(function(product) {
                        return product.ProductID = productId;
                    })[0].ProductSales
                });
                productSalesDataSources[productId] = productSalesDataSource;
            }

            return productSalesDataSource;
        };

        var productDataSources = {};

        this.productDataSource = function(orderId) {
            var productDataSource = productDataSources[orderId];

            if (!productDataSource) {
                productDataSource = new kendo.data.DataSource({
                    data: this.orderDetails.filter(function(product) {
                        return product.OrderID === orderId;
                    }).map(function(product) {
                        var orderDetails = product.OrderDetails[0];
                        var sum = orderDetails.UnitPrice * orderDetails.Quantity;

                        orderDetails.Total = sum - (sum * orderDetails.Discount)

                        return orderDetails;
                    }),
                    aggregate: [
                      { field: "Total", aggregate: "sum" }
                    ]
                });
                productDataSources[orderId] = productDataSource;
            }

            return productDataSource;
        };

        this.ordersColumns = [
            { field: "OrderID", title: "ORDER ID" },
            { field: "OrderDate", title: "ORDER DATE", format: "{0: yyyy-MM-dd}", width: 150 },
            { field: "CustomerID", title: "CUSTOMER" },
            { field: "EmployeeID", title: "EMPLOYEE", values: employees },
            { field: "ShipCountry", title: "SHIP COUNTRY" },
            { field: "ShipVia", title: "SHIP VIA", values: shippers }
        ];

    }]);
