function fixTable() {
    var $header = $("table.foxFix > thead");
    var tableOffset = $("table.foxFix").offset().top - $header.outerHeight() - $header.height();
    var $fixTable = $("#header-fixed");
    var $fixedHeader = $fixTable.append($header.clone());
    function setWidthFixHeader() {
        var w = $header.outerWidth();
        $fixedHeader.css('width', w);
        var thNorm = $header.find('th');
        var thFix = $fixedHeader.find('th');
        for (var i = 0, len = thNorm.length; i < len; i++) {
            w = $(thNorm[i]).outerWidth();
            $(thFix[i]).css('width', w);
        }
    }
    $(window).bind("scroll", function () {
        var offsetV = $(this).scrollTop();
        var offsetH = $(this).scrollLeft();
        // по вертикали
        if (offsetV >= tableOffset && $fixedHeader.is(":hidden")) {
            setWidthFixHeader();
            $fixedHeader.show();
        } else if (offsetV < tableOffset) {
            $fixedHeader.hide();
        }
    });
}