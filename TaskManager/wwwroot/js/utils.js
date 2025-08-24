async function manageApiError (response) {
    let errorMessage = '';

    if (response.status == 400) {
        errorMessage = await response.text();
    } else if (response.status == 404) {
        errorMessage = notFoundResource;
    } else {
        errorMessage = unexpectedError;
    }

    showErrorMessage(errorMessage);
}

function showErrorMessage(message) {
    Swal.fire({
        icon: "error",
        title: "Error...",
        text: message,
    });
}

function confirmAction({ acceptCallBack, cancelCallBack, title }) {
    Swal.fire({
        title: title || "¿Confirmar Acción?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#E33717",
        confirmButtonText: "Confirmar",
        cancelButtonText: "Cancelar",
        focusConfirm: true,
    }).then((result) => {
        if (result.isConfirmed) {
            acceptCallBack();
        } else if (cancelCallBack) { 
            cancelCallBack();
        }
    })
}

function downloadFile(url, name) {
    var link = document.createElement('a');
    link.download = name;
    link.target = "_blank";
    link.href = url;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    delete link;
}