

$(document).ready(function () {

    toastr.options.preventDuplicates = true;
    
    studio.Storage.sessionStorage.setItem("BULK_SEL_TARGET_SEL_LEVEL", "");
    //sessionStorage.setItem("BULK_SEL_NODE_ID", "");

    showScrollToTop();

    /*
    $('.formulary-list #iBulkEdit, .formulary-list #iBulkEditFloat ').click(function () {
        let selectedRecordsAsString = sessionStorage.getItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_IDs);
        if (!selectedRecordsAsString) return;

        let selectedRecords = JSON.parse(selectedRecordsAsString);
        if (!selectedRecords || Object.keys(selectedRecords).length == 0) return;

        let $container = $('#pnlBulkEditSelectorContainer');
        $container.show();
        $container.html('');
        $('#pnlUpdateProgress').show();
        ajaxLoad('/Formulary/GetBulkEditSelectorPartial', null, $container, () => { $('#pnlUpdateProgress').hide(); }, () => { $('#pnlUpdateProgress').hide(); });
    });
    */


    /*
    $("#addMed").on('click', function () {
        $("#btnImport").show();
        $("#btnImport").attr("disabled", true);
        $("#btnImportDisabled").hide();
        $('#modal-spinner').hide();
        $('#chkImportAutoSelect').prop("checked", false);
        $('#medModal').modal('show');
    });
    */
    /*
    $("#customMed").on('click', function () {
        let $container = $('#pnlCreateCustomMedContainer');
        $container.show();
        $container.html('');
        $('#pnlUpdateProgress').show();
        ajaxLoad('/Formulary/GetCustomMedContainerPartial', null, $container, () => { $('#pnlUpdateProgress').hide(); }, () => { $('#pnlUpdateProgress').hide(); });

        //$('#customMedModal').modal('show');
        //$("#dvLoading").show();
        //$("#btnSaveCustomMedDisabled").hide();
        //ajaxLoad('/Formulary/LoadVMPForm',
        //    null,
        //    null,
        //    (data) => {
        //        $("#customMedComponent").html(data);
        //        $("#dvLoading").hide();
        //        $("#ArchiveReasonshow").hide();
        //        $("#lblArchivestatuserror").hide();
        //    }
        //);
    });
    */

    //$("#btnSaveCustomMed").click(function () {

    //    $("#customMedForm").find('input:text').each(function () {
    //        this.value = $(this).val().trim();
    //    });

    //    let formInput = $("#customMedForm").serializeObject();

    //    processAutoCompletes(formInput, '#Create_New_Custom_Medication');
    //    processAdditionalCodes(formInput, '#Create_New_Custom_Medication');
    //    processIngredients(formInput, '#Create_New_Custom_Medication');
    //    processExcipients(formInput, '#Create_New_Custom_Medication');

    //    let validationSmryPanel = $('#Create_New_Custom_Medication div#pnlMedicationValidationSmry');

    //    //This is required - otherwise the hidden fields will be ignored from validation (but required for Autocomplete)
    //    $("#customMedForm").data("validator").settings.ignore = "";
    //    //$.validator.setDefaults({ ignore: '' });

    //    let validator = $("#customMedForm").validate({
    //        //ignore: '',
    //        errorContainer: validationSmryPanel,
    //        errorLabelContainer: $("ul", validationSmryPanel),
    //        wrapper: 'li'
    //    });

    //    let isFormValid = validator.form();
    //    //console.log(isFormValid);

    //    if (!isFormValid) return;

    //    ajaxPost('Formulary/AddCustomMedication', formInput,
    //        (data) => {

    //            if (data != null) {
    //                $('#customMedComponent').html(data);
    //            }
    //            else {
    //                $('#customMedModal').modal('hide');
    //                $('#customMedComponent').html('');
    //            }
    //        }
    //    );
    //});

    $("#btnUpdateCustomMed").click(function () {
        let formInput = $('#editMedForm').serializeObject();

        let isBulkEdit = (formInput['IsBulkEdit'] && (formInput['IsBulkEdit'] == true || formInput['IsBulkEdit'] == 'True'))

        if (isBulkEdit) {
            if ($("#Edit_Formulary_ddlRecordStatus").children("option:selected").val() == "") {
                UpdateFormulary();
            }
            else {
                if ((!$("#RecStatuschangeMsg").val() || $("#RecStatuschangeMsg").val().trim() === "") && $("#Edit_Formulary_ddlRecordStatus").children("option:selected").val() === "004") {
                    $("#lblstatuschgerr").show();
                    return;
                }
                UpdateFormulary();
                // this below has been moved as a part of bulk update formulary and not required
                /*
                let request = {};

                let selectedRecords = sessionStorage.getItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_IDs);

                if (selectedRecords) {
                    let recordsJson;

                    recordsJson = JSON.parse(selectedRecords);

                    jsonObj = [];

                    Object.keys(recordsJson).forEach(function (rec) {
                        item = {}
                        item["FormularyVersionId"] = rec;
                        item["RecordStatusCode"] = $("#Edit_Formulary_ddlRecordStatus").children("option:selected").val();
                        item["RecordStatusCodeChangeMsg"] = $("#RecStatuschangeMsg").val()

                        jsonObj.push(item);
                    });

                    request.RequestData = jsonObj;
                }

                if (request != {} && isBulkEdit) {
                    let url = 'Formulary/BulkUpdateFormularyStatus';

                    ajaxPost(url, request,
                        (data) => {
                            if (data != null && data.length > 0 && data[0] == "success") {
                                UpdateFormulary();
                            }
                            else {
                                if (data != null && data.length > 0) {
                                    toastr.error(data);
                                }
                            }
                        },
                        (err) => {
                            console.log(err);
                        }
                    );
                }*/
            }
        }
        else {
            UpdateFormulary();
        }

    });

    $.fn.serializeObject = function () {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name] !== undefined) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };

    $("#customMedModal").on("hide.bs.modal", function () {
        $('#customMedModal .add-additional-code').off();
        $('#customMedModal .add-ingredient').off();
        $("#customMedComponent").html();

    });

    $('#filterEl')[0].editFormulary = editFormularyNode;
    $('#filterEl')[0].showBulkEdit = () => {
        let selectedRecordsAsString = studio.Storage.sessionStorage.getItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_IDs);
        if (!selectedRecordsAsString) return;

        let selectedRecords = JSON.parse(selectedRecordsAsString);
        if (!selectedRecords || Object.keys(selectedRecords).length == 0) return;

        let $container = $('#pnlBulkEditSelectorContainer');
        $container.show();
        $container.html('');
        $('#pnlUpdateProgress').show();
        ajaxLoad('/Formulary/GetBulkEditSelectorPartial', null, $container, () => { $('#pnlUpdateProgress').hide(); }, () => { $('#pnlUpdateProgress').hide(); });
    };
    $('#filterEl')[0].showReImport = () => {
        $("#btnImport").show();
        $("#btnImport").attr("disabled", true);
        $("#btnImportDisabled").hide();
        $('#modal-spinner').hide();
        $('#chkImportAutoSelect').prop("checked", false);
        $('#medModal').modal('show');
    };
    $('#filterEl')[0].showCustomMedication = () => {
        let $container = $('#pnlCreateCustomMedContainer');
        $container.show();
        $container.html('');
        $('#pnlUpdateProgress').show();
        ajaxLoad('/Formulary/GetCustomMedContainerPartial', null, $container, () => { $('#pnlUpdateProgress').hide(); }, () => { $('#pnlUpdateProgress').hide(); });
    };
    $('#filterEl')[0].showHistory = () => { $('#historyList').prop('canShow', true); };

    $('#filterEl')[0].canShow = true;

    //mmc-477
    onPostImportSuccess = () => {
        console.log('refreshSearch');
        $('#filterEl')[0].refreshsearch = true;
    };

    //$('#filter').click(function () {
    //    $('#filterEl').prop('canShow', true);

    //});

    //$('#history').click(function () {
    //    $('#historyList').prop('canShow', true);
    //    //$('#historyModal').modal('show');
    //});

    $('#historyList')[0].comparehistory = ((historyItem) => {
        //console.log(historyItem);
        GetPreviousFormularyDetails(historyItem.previousFormularyVersionId, historyItem.currentFormularyVersionId);
        GetCurrentFormularyDetails(historyItem.previousFormularyVersionId, historyItem.currentFormularyVersionId);
    });

    $('#historyModal').on('show.bs.modal', function (e) {
        $('#pnlUpdateProgress').show();
        let req = { pageNo: 1, pageSize: 10 }
        getHistoryOfFormularies('Formulary/GetHistoryOfFormularies', req, null, null);
    });

    $("#historyModal").on("hide.bs.modal", function () {
        $(".fancySearchRow").remove();
        $("#tblHistory tbody tr").remove();
        $("#tblHistory tfoot tr").remove();
    });

    //$('#historyModal').on('shown.bs.modal', function (e) {
    //    applyPagingToHistoryTable();
    //});

    //$("#historySearch").on("keyup", function () {
    //    var value = $(this).val();

    //    if (value != "") {
    //        var totalRecords = 0;

    //        $("#tblHistory tbody tr").each(function (index) {
    //            $row = $(this);

    //            var id = $row.find("td:nth-child(3)").text().toLowerCase();

    //            if (id.indexOf(value.toLowerCase()) !== 0) {
    //                $row.hide();
    //            }
    //            else {
    //                $row.show();
    //                totalRecords = totalRecords + 1;
    //            }
    //        });


    //        $('#historyPagination').pagination('updateItems', totalRecords);
    //    }

    //});
});


function showScrollToTop() {
    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) {
            $('#iBulkEditFloat').fadeIn();
            $('#back-to-top').fadeIn();
        } else {
            $('#iBulkEditFloat').fadeOut();
            $('#back-to-top').fadeOut();
        }
    });
    // scroll body to 0px on click
    $('#back-to-top').click(function () {
        $('body,html').animate({
            scrollTop: 0
        }, 400);
        return false;
    });
}

function editFormularyNode(node) {

    $('#pnlUpdateProgress').show();

    let url = "/Formulary/LoadFormularyDetails";
    let args = {
        formularyVersionId: node.data.formularyVersionId || node.data.formularyversionid
    };

    ajaxPost(url, args,
        (data) => {
            $('#pnlUpdateProgress').hide();
            $('#editMedModal').modal('show');
            //$("#inspectorcomp").show();
            $("#inspectorcomp").html(data);
            //$("#ArchiveReasonshow").hide();
            //$("#lblArchivestatuserror").hide();
            //$('#btnsShowinspector').show();
            $('#btnUpdateCustomMed').show();
            $('#btnUpdateCustomMedDisabled').hide();

        },
        (err) => {
            $('#pnlUpdateProgress').hide();
            console.error(err);
        });
}

function bulkEditSelectorClose(updateBulkEdit) {
    //var tree = $.ui.fancytree.getTree("#treetable");
    let selectedDatas = {};

    let hasSelectedData = true;
    var selectedDataString = studio.Storage.sessionStorage.getItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_IDs);

    if (!selectedDataString) hasSelectedData = false;

    if (hasSelectedData)
        selectedDatas = JSON.parse(selectedDataString);

    if (!selectedDatas || Object.keys(selectedDatas).length == 0) hasSelectedData = false;

    if (!hasSelectedData) {
        studio.Storage.sessionStorage.removeItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_IDs);
        studio.Storage.sessionStorage.removeItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_LEVEL);
        studio.Storage.sessionStorage.removeItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_STATUS);

        $('#filterEl')[0].selectFormularies = null;

        //tree.visit(function (treenode) {
        //    treenode.setSelected(false);
        //    treenode.render();
        //    addOrRemoveSelectedNodes(null, null, treenode);
        //});

        return;
    }

    if (!updateBulkEdit) {
        //tree.visit(function (treenode) {
        //    treenode.setSelected(selectedDatas[treenode.data.formularyVersionId] ? true : false);
        //    treenode.render();
        //});

        $('#filterEl')[0].selectFormularies = Object.keys(selectedDatas);

        return;
    }

    let selectedFormularyIds = [];

    Object.keys(selectedDatas).forEach(function (itemKey, index) {
        selectedFormularyIds.push(selectedDatas[itemKey].formularyVersionId);
    });

    if (updateBulkEdit) {

        $('#pnlUpdateProgress').show();

        //let url = "/Formulary/GetProductTypeBulkEditPartial";
        const url = '/Formulary/GetProductTypeBulkEditPartialByPost';
        let args = {
            formularyVersionIds: JSON.stringify(selectedFormularyIds),
            productType: studio.Storage.sessionStorage.getItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_LEVEL),
            status: studio.Storage.sessionStorage.getItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_STATUS)
        };

        //ajaxLoad(url, args, $("#inspectorcomp"),
        ajaxPost(url, args,
            (data) => {
                $('#pnlUpdateProgress').hide();
                $('#editMedModal').modal('show');
                //$("#inspectorcomp").show();
                //$("#inspectorcomp").html(data);
                //$("#ArchiveReasonshow").hide();
                //$("#lblArchivestatuserror").hide();
                //$('#btnsShowinspector').show();
                $('#btnUpdateCustomMed').show();
                $('#btnUpdateCustomMedDisabled').hide();

            },
            (err) => {
                $('#pnlUpdateProgress').hide();
                console.error(err);
            }, $("#inspectorcomp"));
    }

}

//function getAssignableStatuses(currentStatus) {
//    if (!currentStatus || currentStatus === 'Archived') return [];
//    switch (currentStatus) {
//        case 'Draft':
//            return [{ val: '002', text: 'Approve' }];
//        case 'Approved':
//            return [{ val: '003', text: 'Active' }];
//        case 'Active':
//            return [{ val: '005', text: 'Inactive' }, { val: '004', text: 'Archive' }];
//        case 'Inactive':
//            return [{ val: '003', text: 'Active' }, { val: '004', text: 'Archive' }];
//        default:
//            return [];
//    }
//}




function showFormularyCntrlTooltip(item) {
    $(item).data('html', true);
    $(item).data('title', $(item).data('tooltip-content'));
    $(item).tooltip('toggle');
}


// EDIT: Let's cover URL fragments (i.e. #page-3 in the URL).
// More thoroughly explained (including the regular expression) in: 
// https://github.com/bilalakil/bin/tree/master/simplepagination/page-fragment/index.html

// We'll create a function to check the URL fragment
// and trigger a change of page accordingly.
function checkFragment() {
    // If there's no hash, treat it like page 1.
    var hash = window.location.hash || "#page-1";

    // We'll use a regular expression to check the hash string.
    hash = hash.match(/^#page-(\d+)$/);

    if (hash) {
        // The `selectPage` function is described in the documentation.
        // We've captured the page number in a regex group: `(\d+)`.
        $("#historyPagination").pagination("selectPage", parseInt(hash[1]));
    }
};

function getHistoryOfFormularies(url, args, beforeLoad, afterLoad) {
    if (beforeLoad) {
        beforeLoad();
    }
    ajaxGetJson(url, args,
        (data) => {
            if (afterLoad) {
                afterLoad(true);
            }

            if (!data || data.length == 0) return;

            var str = [];

            data.forEach((item) => {
                if (item.status == "Active" || item.status == "") {
                    var htmlString = "<tr><td>" + item.dateTime + "</td><td>" + item.user + "</td><td>" + item.name + " (" + (item.productType) + ")" + "</td><td>" + item.status + "</td><td><button type='button' class='btn btn-secondary' onclick='GetPreviousFormularyDetails(\"" + item.previousFormularyVersionId + "\", \"" + item.currentFormularyVersionId + "\");GetCurrentFormularyDetails(\"" + item.previousFormularyVersionId + "\", \"" + item.currentFormularyVersionId + "\");'>Properties Changed</button></td></tr>";
                    str.push(htmlString);
                }
                if (item.status == "Draft") {
                    var htmlString = "<tr><td>" + item.dateTime + "</td><td>" + item.user + "</td><td>" + item.name + " (" + (item.productType) + ")" + "</td><td>" + item.status + "</td><td>Imported</td></tr>";
                    str.push(htmlString);
                }
                if (item.status == "Deleted") {
                    var htmlString = "<tr><td>" + item.dateTime + "</td><td>" + item.user + "</td><td>" + item.name + " (" + (item.productType) + ")" + "</td><td>" + item.status + "</td><td>Deleted</td></tr>";
                    str.push(htmlString);
                }
            });

            $('#historyOutput').html(str);

            //applyPagingToHistoryTable();
            //$("#tblHistory").fancyTable({
            //    sortable: false,
            //    pagination: true,
            //    perPage: 15,
            //    globalSearch: false
            //});

            $('#tblHistory').DataTable().destroy();

            var foot = $("#tblHistory").find('tfoot');

            if (!foot.length) foot = $('<tfoot>').appendTo('#tblHistory');

            foot.append($('<tr><th scope="col">Date/Time</th><th scope="col">User</th><th scope="col">Product Name & Code</th><th scope="col">Status</th><th scope="col">Updated</th></tr>'));

            $('#tblHistory tfoot').css('display', 'table-header-group');

            // Setup - add a text input to each footer cell
            $('#tblHistory tfoot th').each(function () {
                var title = $(this).text();
                $(this).html('<input type="text" placeholder="Search ' + title + '" />');
            });

            // DataTable
            $('#tblHistory').DataTable({
                dom: 'lrt',
                ordering: false,
                initComplete: function () {
                    // Apply the search
                    this.api()
                        .columns()
                        .every(function () {
                            var that = this;

                            $('input', this.footer()).on('keyup change clear', function () {
                                if (that.search() !== this.value) {
                                    that.search(this.value).draw();
                                }
                            });
                        });
                },
            });

            $('#tblHistory thead').children().slice(1).remove();

            $('#tblHistory tfoot tr').appendTo('#tblHistory thead');

            $('#pnlUpdateProgress').hide();
        });
}

function applyPagingToHistoryTable() {
    var tblHistory = $('#tblHistory tbody tr');

    var numItems = tblHistory.length;
    var perPage = 15;

    // Only show the first 2 (or first `per_page`) items initially.
    tblHistory.slice(perPage).hide();

    // Now setup the pagination using the `.pagination-page` div.
    $("#historyPagination").pagination({
        items: numItems,
        itemsOnPage: perPage,
        cssStyle: "light-theme",

        // This is the actual page changing functionality.
        onPageClick: function (pageNumber) {
            // We need to show and hide `tr`s appropriately.
            var showFrom = perPage * (pageNumber - 1);
            var showTo = showFrom + perPage;

            // We'll first hide everything...
            tblHistory.hide()
                // ... and then only show the appropriate rows.
                .slice(showFrom, showTo).show();
        }
    });

    // We'll call this function whenever back/forward is pressed...
    $(window).bind("popstate", checkFragment);

    // ... and we'll also call it when the page has loaded
    // (which is right now).
    checkFragment();
}

function GetPreviousFormularyDetails(previousFormularyVersionId, currentFormularyVersionId) {
    $('#pnlUpdateProgress').show();

    let url = "/Formulary/LoadHistoryFormularyDetails";
    let args = {
        previousFormularyVersionId: previousFormularyVersionId,
        currentFormularyVersionId: currentFormularyVersionId,
        previousOrCurrent: "previous"
    };

    ajaxPost(url, args,
        (data) => {
            if (!data) {
                $('#pnlUpdateProgress').hide();
                return;
            }
            $('#historyModal').modal('hide');
            $('#pnlUpdateProgress').hide();
            $('#historyMedModal').modal('show');
            $("#previous").html(data);
        },
        (err) => {
            $('#pnlUpdateProgress').hide();
            console.error(err);
        });
}

function GetCurrentFormularyDetails(previousFormularyVersionId, currentFormularyVersionId) {
    $('#pnlUpdateProgress').show();

    let url = "/Formulary/LoadHistoryFormularyDetails";
    let args = {
        previousFormularyVersionId: previousFormularyVersionId,
        currentFormularyVersionId: currentFormularyVersionId,
        previousOrCurrent: "current"
    };

    ajaxPost(url, args,
        (data) => {
            if (!data) {
                $('#pnlUpdateProgress').hide();
                return;
            }
            $('#historyModal').modal('hide');
            $('#pnlUpdateProgress').hide();
            $('#historyMedModal').modal('show');
            $("#current").html(data);
        },
        (err) => {
            $('#pnlUpdateProgress').hide();
            console.error(err);
        });
}

function UpdateFormulary() {

    let formularySaveObj = new studio.FormularySave('#editMedForm', '#Edit_Formulary');

    formularySaveObj.update(
        () => {
            $('#btnUpdateCustomMed').hide();
            $('#btnUpdateCustomMedDisabled').show();
            $('#pnlUpdateProgress').show();
        },
        (data, formInput) => {
            let isBulkEdit = (formInput['IsBulkEdit'] && (formInput['IsBulkEdit'] == true || formInput['IsBulkEdit'] == 'True'))

            if (data != null) {
                $('#inspectorcomp').html(data);
                $('#btnUpdateCustomMed').show();
                $('#btnUpdateCustomMedDisabled').hide();
                $('#pnlUpdateProgress').hide();
                $('#editMedModalBody').animate({
                    scrollTop: 0
                }, 'fast');
            }
            else {
                $('#inspectorcomp').html('');
                $('#btnUpdateCustomMed').show();
                $('#btnUpdateCustomMedDisabled').hide();
                $('#pnlUpdateProgress').hide();
                $('#editMedModal').modal('hide');
                if (isBulkEdit) { //Saved successfully - so refresh tree
                    studio.Storage.sessionStorage.removeItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_IDs);
                    studio.Storage.sessionStorage.removeItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_LEVEL);
                    studio.Storage.sessionStorage.removeItem(studio.Storage.STORAGE_KEYS.FORMULARY_BULK_SELECT_STATUS);
                    $('#lblBulkEditNumber, #lblBulkEditNumbeFloat').html('0');
                    $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');

                    $('#filterEl')[0].selectFormularies = null;
                }
                //callSearchFormularies();
                onPostImportSuccess();
            }

            //MMC-477 - Fix01 - Not required here - only if we do not get 'data'
            //$('#filterEl')[0].selectFormularies = null;
        },
        (err, formInput) => {

            if (err) {
                console.error(err);
            }
            $('#inspectorcomp').html('');
            $('#btnUpdateCustomMed').show();
            $('#btnUpdateCustomMedDisabled').hide();
            $('#pnlUpdateProgress').hide();
            $('#editMedModalBody').animate({
                scrollTop: 0
            }, 'fast');
            $('#editMedModal').modal('hide');

        });
}