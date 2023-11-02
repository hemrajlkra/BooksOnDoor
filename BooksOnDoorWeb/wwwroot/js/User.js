var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#lblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', "width": "20%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'company.name', "width": "10%" },
            { data: 'role', "width": "10%" },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return `
                        <div class="text-center" >
                             <a onClick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer;width:100px;">
                                <i class="bi bi-lock-fill"></i>&nbsp;Lock
                             </a>
                            <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer;width:150px;">
                                <i class="bi bi-pencil-square"></i>&nbsp; Permission
                            </a>
                        </div>`
                    }
                    else {
                        return `
                        <div class="text-center" >
                            <a onClick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor:pointer;width:100px;">
                                <i class="bi bi-unlock-fill"></i>&nbsp;Unlock
                            </a>
                            <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer;width:150px;">
                                <i class="bi bi-pencil-square"></i>&nbsp; Permission
                            </a>
                        </div>`
                    }
                },
                "width":"25%"
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
    })
}
