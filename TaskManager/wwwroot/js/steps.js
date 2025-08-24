function handlerClickAddStep() {

    const index = editTaskViewModel.steps().findIndex(s => s.isNew());

    if (index !== -1) {
        return;
    }

    editTaskViewModel.steps.push(new stepViewModel({ done: false, editionMode: true }));
    $("[name=txtDescriptionStep]:visible").focus();
}

function handlerClickCancelSaveStep(step) { 
    if (step.isNew()) {
        editTaskViewModel.steps.pop();
    } else {
        step.description(step.previousDescription);
        step.editionMode(false);
    }
}

async function handlerClickSaveStep(step) {
    step.editionMode(false);

    const isNew = step.isNew();
    const taskId = editTaskViewModel.id;
    const data = getStepRequestBody(step);

    const description = step.description();

    if (!description) {
        step.description(step.previousDescription);

        if (step.isNew()) {
            editTaskViewModel.steps.pop();
        }
        return;
    }

    if (isNew) {
        await insertNewStep(step, data, taskId);
    } else {
        await updateStep(data, step.id());
    }


}

async function insertNewStep(step,data, taskId) {
    const response = await fetch(`${urlSteps}/${taskId}`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const json = await response.json();
        step.id(json.id);

        const task = getTaskInEdition();

        task.totalSteps(task.totalSteps()+1);
        if (step.done()) {
            task.doneSteps(task.totalSteps()+1);
        }
    } else {
        manageApiError(response);
    }
}

function getStepRequestBody(step) {
    return JSON.stringify({
        description: step.description(),
        done: step.done()
    });
}

function handlerClickStep(step) {
    step.editionMode(true);
    step.previousDescription = step.description();
    $("[name=txtDescriptionStep]:visible").focus();
}

async function updateStep(data, id) {
    const response = await fetch(`${urlSteps}/${id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }

    const task = getTaskInEdition();
    const step = await response.json();

    if (step.done) {
        task.doneSteps(task.doneSteps()+1);
    } else {
        task.doneSteps(task.doneSteps()-1);
    }
}
function handlerClickDeleteStep(step) {
    editModalTaskBootstrap.hide();

    confirmAction({
        acceptCallBack: () => {
            deleteStep(step.id());
            editModalTaskBootstrap.show();
        },
        cancelCallBack: () => {
            editModalTaskBootstrap.show();
        },
        title: `¿Desea borrar el paso ${step.description()}?`
    })
}

async function deleteStep(id) {
    const response = await fetch(`${urlSteps}/${id}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }
    const task = getTaskInEdition();

    task.totalSteps(task.totalSteps()-1);
    const step = getStepInEdition();
    if (step.done()) {;
        task.doneSteps(task.doneSteps()-1);
    }

    const index = getStepEditionIndex();
    editTaskViewModel.steps.splice(index, 1);
}

function getStepEditionIndex() {
    return editTaskViewModel.steps().findIndex(step => step.editionMode() == true);
}

function getStepInEdition() {
    const index = getStepEditionIndex();
    return editTaskViewModel.steps()[index];
}

$(function () {
    $("#reorderable-steps").sortable({
        axis: 'y',
        stop: async function () {
            await updateStepOrder();
        }
    })
})

async function updateStepOrder() {
    const ids = getStepIds();
    await sendStepIds(ids);

    const sortedList = editTaskViewModel.steps.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    editTaskViewModel.steps([]);
    editTaskViewModel.steps(sortedList);
}

function getStepIds() {
    const ids = $("[name=checkBoxStep]").map(function () {
        return $(this).attr("data-id");
    }).get();
    return ids;
}

async function sendStepIds(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlSteps}/sort/${editTaskViewModel.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

}