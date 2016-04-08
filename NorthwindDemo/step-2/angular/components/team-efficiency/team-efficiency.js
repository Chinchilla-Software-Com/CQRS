angular.module('app.team', [])
    .controller('TeamEfficiencyController', ['$q', 'EmployeeList', 'EmployeeSales', 'EmployeeTeamSales', 'EmployeeAverageSales', 'EmployeeQuarterSales',
    function ($q, EmployeeList, EmployeeSales, EmployeeTeamSales, EmployeeAverageSales, EmployeeQuarterSales) {
        this.startDate = new Date(1996, 0, 1);

        this.endDate = new Date(1998, 7, 1);

        this.currentDate = new Date(1996, 6, 1);

        this.currentEmployee = null;

        var employeeTeamSales = EmployeeTeamSales.query();

        var employeeAverageSales = EmployeeAverageSales.query();

        var employeeQuarterSales = EmployeeQuarterSales.query();

        var employeeList = EmployeeList.query();

        var employeeSales = EmployeeSales.query();

        this.employeeListDataSource = new kendo.data.DataSource();

        this.employeeTeamSalesDataSource = new kendo.data.DataSource();

        this.employeeQuarterSalesDataSource = new kendo.data.DataSource();

        this.employeeAverageSalesDataSource = new kendo.data.DataSource({
            aggregate: [{
                field: 'EmployeeSales',
                aggregate: 'average'
            }]
        });

        this.employeeSalesDataSource = new kendo.data.SchedulerDataSource();

        this.changeCurrentEmployee = function(employee) {
            this.currentEmployee = employee;

            var currentEmployeeQuarterSales = employeeQuarterSales.filter(function(sale) {
                return sale.EmployeeID == employee.EmployeeID;
            })[0].Sales;

            this.currentEmployeeQuarterSales = currentEmployeeQuarterSales[0].Current;

            this.employeeQuarterSalesDataSource.data(currentEmployeeQuarterSales);

            var currentEmployeeTeamSales = employeeTeamSales.filter(function(sale) {
                return sale.EmployeeID == employee.EmployeeID;
            })[0].Sales;

            this.employeeTeamSalesDataSource.data(currentEmployeeTeamSales);

            var currentEmployeeAverageSales = employeeAverageSales.filter(function(sale){
                return sale.EmployeeID == employee.EmployeeID;
            });

            this.employeeAverageSalesDataSource.data(currentEmployeeAverageSales);

            var aggregates = this.employeeAverageSalesDataSource.aggregates();

            this.currentEmployeeAverageSalesNumber = aggregates.EmployeeSales ? aggregates.EmployeeSales.average : 0;

            var currentEmployeeSales = employeeSales.filter(function(sale) {
                return sale.EmployeeID == employee.EmployeeID;
            }).map(function(sale) {
                return {
                    description: sale.Description,
                    start: kendo.parseDate(sale.Start),
                    title: sale.Title,
                    end: kendo.parseDate(sale.End)
                };
            });

            this.employeeSalesDataSource.data(currentEmployeeSales);
        };

        $q.all([employeeQuarterSales.$promise, employeeList.$promise, employeeAverageSales.$promise, employeeTeamSales.$promise, employeeSales.$promise]).then(function() {
            this.employeeListDataSource.data(employeeList);
            this.changeCurrentEmployee(employeeList[0]);
        }.bind(this));
    }]);
