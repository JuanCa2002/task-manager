function addNewTask() {
    taskListViewModel.tasks.push(new elementTaskViewModel({ id: 0, title: '' }))

    $("[name=title]").last().focus();
}

async function manageFocusOutTaskTitle(task) {
    const title = task.title();
    if (!title.trim()) {
        taskListViewModel.tasks.remove(task);
        return;
    }
    const data = JSON.stringify(title);
    const response = await fetch(urlTasks, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const json = await response.json();
        task.id(json.id);
    } else {
        manageApiError(response);
    }
}

async function getTaskList() {
    taskListViewModel.loading(true);

    const response = await fetch(urlTasks, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }

    const json = await response.json();
    taskListViewModel.tasks([]);

    json.forEach(value => {
        taskListViewModel.tasks.push(new elementTaskViewModel(value));
    })

    taskListViewModel.loading(false);

}

async function updateTaskOrder() {
    const ids = getTaskIds();
    await sendIds(ids);

    const sortedList = taskListViewModel.tasks.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    taskListViewModel.tasks([]);
    taskListViewModel.tasks(sortedList);
}

function getTaskIds() {
    const ids = $("[name=title]").map(function () {
        return $(this).attr("data-id");
    }).get();
    return ids;
}

async function sendIds(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTasks}/sort`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

}

$(function () {
    $("#reorderable").sortable({
        axis: 'y',
        stop: async function () {
            await updateTaskOrder();
        }
    })
})

async function manageTaskClick(task) {
    if (task.isNew()) {
        return;
    }

    const response = await fetch(`${urlTasks}/${task.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }

    const json = await response.json();

    editTaskViewModel.id = json.id;
    editTaskViewModel.title(json.title);
    editTaskViewModel.description(json.description);
    editTaskViewModel.steps([]);

    json.steps.forEach(step => {
        editTaskViewModel.steps.push(
            new stepViewModel({...step, editionMode: false}));
    });

    editTaskViewModel.attachedFiles([]);

    prepareAttachedFiles(json.attachedFiles);

    editModalTaskBootstrap.show();
}

async function handlerEditTaskChange() {
    const task = {
        id: editTaskViewModel.id,
        title: editTaskViewModel.title(),
        description: editTaskViewModel.description()
    }

    if (!task.title) {
        return;
    }

    await editFullTask(task);

    const index = taskListViewModel.tasks().findIndex(t => t.id() == task.id);
    const currentTask = taskListViewModel.tasks()[index];
    currentTask.title(task.title);


}

async function editFullTask(task) {
    const data = JSON.stringify(task);
    const response = await fetch(`${urlTasks}/${task.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        manageApiError(response);
        throw "Error";
    }

}

function showDeleteModal(task) {
    editModalTaskBootstrap.hide();
    confirmAction({
        acceptCallBack: () => {
           deleteTask(task.id);
        },
        cancelCallBack: () => {
            editModalTaskBootstrap.show();
        },
        title: `¿Desea borrar la tarea ${task.title()}?`
    })
}

async function deleteTask(id) {
    const response = await fetch(`${urlTasks}/${id}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }
    const index = getTaskEditionIndex();
    taskListViewModel.tasks.splice(index, 1);
}

function getTaskEditionIndex() {
    return taskListViewModel.tasks().findIndex(task => task.id() == editTaskViewModel.id);
}

function getTaskInEdition() {
    const index = getTaskEditionIndex();
    return taskListViewModel.tasks()[index];
}