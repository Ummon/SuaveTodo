(function() {  
    var app = angular.module("Todo", []);
  
    app.controller("TodoList", function($scope, $http) {    
        var self = this;
      
        self.text = "";
        self.todos = [];
     
        var load = function() { 
            $http.get('/todos').success(function(data) {
                self.todos = data.todos;
            }); 
        };

        self.create = function () {
            var text = self.text; 
            $http.post('/todos', "text=" + encodeURIComponent(text)).then(function() {
                self.text = "";
                load();
            });
        };
      
        self.remove = function (id) {
            $http.delete('/todos/' + id).then(function() {
                load();
            });
        };
      
        load();
    });
})();