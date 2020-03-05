function showLoader() {
    $("#dvLoading").show();
}

function hideLoader() {
    $("#dvLoading").hide();
}

$.ajaxSetup({
    error: function (xhr, props) {
        if (xhr.status === 401) {
            location.reload();
        }
    }
});

function ConvertUTCToLocal(strdatetime, format, outputFormat) {
    if (format === undefined || format == null || format == '')
        format = 'DD/MM/YYYY hh:mm A';

    if (outputFormat === undefined || outputFormat == null || outputFormat == '')
        outputFormat = 'DD/MM/YYYY hh:mm A';

    var utcDT = moment.utc(strdatetime, format);
    var localDT = utcDT.local().format(outputFormat);
    return localDT;
}

function ChangeDateFormat(strdatetime, format, outputFormat) {
    if (format === undefined || format == null || format == '')
        format = 'DD/MM/YYYY hh:mm A';

    if (outputFormat === undefined || outputFormat == null || outputFormat == '')
        outputFormat = 'DD/MM/YYYY hh:mm A';

    var currentDT = moment(strdatetime, format);
    var finalDT = currentDT.format(outputFormat);
    return finalDT;
}
