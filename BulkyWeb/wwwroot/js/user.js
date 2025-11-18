var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', width: "25%" },
            { data: 'email', width: "15%" },
            { data: 'phoneNumber', width: "15%" },
            { data: 'company.name', width: "15%" },
            { data: 'role', width: "15%" },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                render: function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        // USER IS LOCKED
                        return `
                            <div class="d-flex justify-content-center gap-2">
                                <a onclick =LockUnlock('${data.id}') class="btn btn-success btn-sm d-flex align-items-center gap-1">
                                    <i class="bi bi-unlock-fill"></i>
                                    Unlock
                                </a>
                                <a href="/admin/user/RoleMangement?id=${data.id}" class="btn btn-primary btn-sm d-flex align-items-center gap-1">
                                    <i class="bi bi-shield-lock-fill"></i>
                                    Permission
                                </a>
                            </div>`
                    } else {
                        // USER IS UNLOCKED
                        return `
                            <div class="d-flex justify-content-center gap-2">
                                <a onclick =LockUnlock('${data.id}') class="btn btn-danger btn-sm d-flex align-items-center gap-1">
                                    <i class="bi bi-lock-fill"></i>
                                    Lock
                                </a>
                                <a href="/admin/user/RoleMangement?id=${data.id}" class="btn btn-primary btn-sm d-flex align-items-center gap-1">
                                    <i class="bi bi-shield-fill"></i>
                                    Permission
                                </a>
                            </div>`
                    }
                },
                width: "25%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}