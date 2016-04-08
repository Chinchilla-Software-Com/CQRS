var app = angular.module('app', ['ngNewRouter', 'ngResource', 'app.regional', 'app.about', 'app.team', 'app.products', 'kendo.directives']).controller('AppController', ['$router', AppController]);

app.factory('Customers', ['$resource', function($resource) {
    return $resource('./Content/customers.json');
}])
.factory('ProductDetails', ['$resource', function($resource) {
    return $resource('./Content/product-details.json');
}])
.factory('EmployeeList', ['$resource', function($resource) {
    return $resource('./Content/employees-list.json');
}])
.factory('EmployeeSales', ['$resource', function($resource) {
    return $resource('./Content/employee-sales.json');
}])
.factory('EmployeeTeamSales', ['$resource', function($resource) {
    return $resource('./Content/employee-and-team-sales.json');
}])
.factory('EmployeeAverageSales', ['$resource', function($resource) {
    return $resource('./Content/employee-average-sales.json');
}])
.factory('EmployeeQuarterSales', ['$resource', function($resource) {
    return $resource('./Content/employee-quarter-sales.json');
}])
.factory('ProductSales', ['$resource', function($resource) {
    return $resource('./Content/product-sales.json');
}])
.factory('Orders', ['$resource', function($resource) {
    return $resource('./Content/orders.json');
}])
.factory('OrderInformation', ['$resource', function($resource) {
    return $resource('./Content/order-information.json');
}])
.factory('CountryCustomers', ['$resource', function($resource) {
    return $resource('./Content/country-customers.json');
}])
.factory('TopSellingProducts', ['$resource', function($resource) {
    return $resource('./Content/top-selling-products.json');
}])
.factory('scale', function() {
    return chroma.scale(["#ade1fb", "#097dc6"]).domain([1, 100]);
})
.factory('OrderDetails', ['$resource', function($resource) {
    return $resource('./Content/order-details.json');
}]);

AppController.$routeConfig = [
    { path: '/', redirectTo: '/regional-sales' },
    { path: '/regional-sales', component: 'regionalSales' },
    { path: '/products-orders', component: 'productsOrders' },
    { path: '/team-efficiency', component: 'teamEfficiency' },
    { path: '/about', component: 'about' }
];

function AppController ($router) {

}
