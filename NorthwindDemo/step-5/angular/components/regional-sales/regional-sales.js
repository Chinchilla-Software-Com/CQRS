
angular.module('app.regional', [])
    .controller('RegionalSalesController', ['$q', 'Customers', 'CountryCustomers', 'OrderDetails', 'TopSellingProducts', 'scale', function ($q, Customers, CountryCustomers, OrderDetails, TopSellingProducts, scale) {
        this.selectedCountry = 'USA';

        this.startDate = new Date(1996, 0, 1);

        this.endDate = new Date(1998, 7, 1);

        this.customers = Customers.query();

        this.countryCustomers = CountryCustomers.query();

        this.marketDataSource = new kendo.data.DataSource();

        this.revenueDataSource = new kendo.data.DataSource();

        this.ordersDataSource = new kendo.data.DataSource();

        this.customersDataSource = new kendo.data.DataSource();

        this.topSellingProductsDataSource = new kendo.data.DataSource({ group: "productName"});

        this.orderDetails = OrderDetails.query();

        this.topSellingProducts = TopSellingProducts.query();

        this.refresh = function() {
            this.currentCustomers = this.customersForCountry();
            this.currentOrders = this.ordersForCountry();
            this.market = this.marketShare();
            var percentage = 0;
            var revenue = 0;
            if (this.market.length > 1) {
                percentage =  this.market[1].sales / this.market[0].sales;
                revenue = this.market[1].sales;
            }
            this.percentage = kendo.toString(percentage, "p2");
            this.revenue = kendo.toString(revenue, "c2");
            this.marketDataSource.data(this.market);
            var revenueData = this.ordersForCountry().map(function(order) {
                return {
                    date: order.orderDate,
                    value: order.price
                };
            });

            this.revenueDataSource.data(revenueData);

            var ordersData = this.ordersForCountry().map(function(order) {
                return {
                    date: order.orderDate,
                    value: 1
                };
            });

            this.ordersDataSource.data(ordersData);

            var customersData = this.countryCustomers.filter(function(customer) {
                var date = kendo.parseDate(customer.Date);
                return customer.Country === this.selectedCountry && date > this.startDate && date < this.endDate;
            }.bind(this)).map(function(customer) {
                return {
                    date: customer.Date,
                    value: customer.Value
                };
            });

            this.customersDataSource.data(customersData);

            this.currentTopSellingProducts = this.topSellingProducts.filter(function(product) {
                var date = kendo.parseDate(product.Date);
                return product.Country === this.selectedCountry && date > this.startDate && date < this.endDate;
            }.bind(this)).map(function(product) {
                return {
                    productName: product.ProductName,
                    date: product.Date,
                    quantity: product.Quantity
                }
            });

            this.topSellingProductsDataSource.data(this.currentTopSellingProducts);
        };

        $q.all([this.customers.$promise, this.countryCustomers.$promise, this.orderDetails.$promise, this.topSellingProducts.$promise]).then(this.refresh.bind(this));

        this.customersForCountry = function() {
            return this.customers.filter(function(customer) {
                return customer.Country === this.selectedCountry;
            }.bind(this)).map(function(customer) {
                return customer.CompanyName;
            });
        };

        this.ordersForCountry = function() {
            return this.orderDetails.filter(function(order) {
                var orderDate = kendo.parseDate(order.orderDate);

                return order.country === this.selectedCountry && orderDate > this.startDate && orderDate < this.endDate;
            }.bind(this));
        };

        this.marketShare = function() {
            var sum = this.currentOrders.reduce(function(total, order) {
                return total + order.price;
            }, 0);

            var market = [
                { country: "All", sales: 854648.019191742 },
            ];

            if (sum > 0) {
                market.push({ country: this.selectedCountry, sales: sum  });
            }

            return market;
        };

        this.mapLayers = [{
            style: {
                fill: {
                    color: "#1996E4"
                },
                stroke: {
                    color: "#FFFFFF"
                }
            },
            type: "shape",
            dataSource: {
                type: "geojson",
                transport: {
                    read: {
                        dataType: "json",
                        url: "./Content/countries-sales.geo.json"
                    }
                }
            }
        }];

        this.shapeCreated = function(e) {
            var color = scale(e.shape.dataItem.properties.sales).hex();
            e.shape.fill(color);
        };

        this.shapeMouseEnter = function(e) {
            e.shape.options.set("fill.color", "#0c669f");
        };

        this.shapeMouseLeave = function(e) {
            if (!e.shape.dataItem.properties.selected) {
                var sales = e.shape.dataItem.properties.sales;
                var color = scale(sales).hex();
                e.shape.options.set("fill.color", color);
                e.shape.options.set("stroke.color", "white");
            }
        };

        this.selectedShape = null;

        this.shapeClick = function(e) {
            if (this.selectedShape) {
                var sales = this.selectedShape.dataItem.properties.sales;
                var color = scale(sales).hex();
                this.selectedShape.options.set("fill.color", color);
                this.selectedShape.options.set("stroke.color", "white");
                this.selectedShape.dataItem.properties.selected = false;
            }

            e.shape.options.set("fill.color", "#0c669f");
            e.shape.options.set("stroke.color", "black");
            e.shape.dataItem.properties.selected = true;
            this.selectedShape = e.shape;
            this.selectedCountry = this.selectedShape.dataItem.properties.name;
            this.refresh();
        }.bind(this);
    }]);
