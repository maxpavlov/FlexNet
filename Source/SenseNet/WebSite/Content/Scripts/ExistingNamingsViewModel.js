function Naming(data) {
    this.nameBlocks = ko.observableArray(data.nameBlocks);
    this.paramBlocks = ko.observableArray(data.paramBlocks);
}

function TaskListViewModel() {
    
    var self = this;
    self.namings = ko.observableArray([]);
//    self.newTaskText = ko.observable();
//    self.incompleteTasks = ko.computed(function () {
//        return ko.utils.arrayFilter(self.tasks(), function (task) { return !task.isDone() && !task._destroy });
//    });

//    // Operations
//    self.addTask = function () {
//        self.tasks.push(new Task({ title: this.newTaskText() }));
//        self.newTaskText("");
//    };
//    self.removeTask = function (task) { self.tasks.destroy(task) };

//    $.getJSON("/tasks", function (allData) {
//        var mappedTasks = $.map(allData, function (item) { return new Task(item) });
//        self.tasks(mappedTasks);
//    });
}