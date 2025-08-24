let inputAttachedFile = document.getElementById("attachedFile");

function handlerClickAddAttachedFile() {
    inputAttachedFile.click();
}

async function handlerSelectionFile(event) {
    const files = event.target.files;
    const filesArray = Array.from(files);

    const taskId = editTaskViewModel.id;
    const formData = new FormData();

    for (var i = 0; i < filesArray.length; i++) {
        formData.append("files", filesArray[i]);
    }

    const response = await fetch(`${urlFiles}/${taskId}`, {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }

    const json = await response.json();
    prepareAttachedFiles(json);

    inputAttachedFile.value = null;
}

function prepareAttachedFiles(files) {
    files.forEach(file => {
        let creationDate = file.creationDate;
        if (file.creationDate.indexOf("Z") === -1) {
            creationDate += "Z";
        }

        const creationDateDT = new Date(creationDate);
        file.published = creationDateDT.toLocaleString();

        editTaskViewModel.attachedFiles.push(
            new attachedFileViewModel({ ...file, editionMode: false}));
    })
}

let previousTitle;
function handlerClickAttachedFile(file) {
    previousTitle = file.title();
    file.editionMode(true);
    $("[name='txtAttachedFileTitle']:visible").focus();
}

async function handlerFocusOut(file) {
    file.editionMode(false);

    if (!file.title()) {
        file.title(previousTitle);
    }

    if (file.title() === previousTitle) {
        return;
    }

    const data = JSON.stringify(file.title());

    const response = await fetch(`${urlFiles}/${file.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': "application/json"
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }
}

function handlerClickDelete(file) {
    editModalTaskBootstrap.hide();
    confirmAction({
        acceptCallBack: () => {
            deleteAttachedFile(file.id);
        },
        cancelCallBack: () => {
            editModalTaskBootstrap.show();
        },
        title: `¿Desea borrar el archivo adjunto ${file.title()}?`
    });
}

async function deleteAttachedFile(id) {
    const response = await fetch(`${urlFiles}/${id}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': "application/json"
        }
    });

    if (!response.ok) {
        manageApiError(response);
        return;
    }

    const index = editTaskViewModel.attachedFiles().findIndex(file => file.id === id);
    editTaskViewModel.attachedFiles.splice(index, 1);
    editModalTaskBootstrap.show();
}

function handlerClickDownload(file) {
    downloadFile(file.url, file.title());
}

$(function () {
    $("#reorderable-attached-files").sortable({
        axis: 'y',
        stop: async function () {
            await updateAttachedFilesOrder();
        }
    })
})

async function updateAttachedFilesOrder() {
    const ids = getAttachedFileIds();
    await sendAttachedFileIds(ids);

    const sortedList = editTaskViewModel.attachedFiles.sorted(function (a, b) {
        return ids.indexOf(a.id.toString()) - ids.indexOf(b.id.toString());
    });

    editTaskViewModel.attachedFiles([]);
    editTaskViewModel.attachedFiles(sortedList);
}

function getAttachedFileIds() {
    const ids = $("[name=txtAttachedFileTitle]").map(function () {
        return $(this).attr("data-id");
    }).get();
    return ids;
}

async function sendAttachedFileIds(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlFiles}/sort/${editTaskViewModel.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

}