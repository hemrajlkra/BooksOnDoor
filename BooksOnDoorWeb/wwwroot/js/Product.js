$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#lblData').DataTable({
        "ajax": { url: '/admin/product/getall' },
        "columns": [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "20%" },
            { data: 'author', "width": "15%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'category.name', "width": "20%" },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group" >
                        <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>&nbsp; Edit</a>
                        <a href="/admin/product/delete?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-trash3"></i>&nbsp; Delete</a>
                    </div>`

                    
                }
            }
        ]

    });
}