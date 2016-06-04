(function ($) {
    var settings = {
        pattern: /\.[0-9a-z]+$/i,
        knownFileTypes: ['pdf', 'png', 'jpg', 'gif', 'bmp', 'doc', 'xls', 'ppt', 'docx', 'xlsx', 'txt', 'pptx', 'zip', 'rar', 'gzip', 'arj', 'wav', 'mp3', 'aif', 'aiff', 'm4a', 'ogg', 'wma', 'psd', 'ai', 'swf', 'fla', 'css', 'js', 'avi', 'mov', 'wmv']
    };
    var methods = {
        init: function (options) {
            jQuery.extend(settings, settings, options);
            return this.each(function () {
                var ext = $(this).attr('href').toLowerCase().match(settings.pattern);

                if (ext != null) {
                    if (ext.length > 0) {
                        ext = ext[0].slice(1);
                    }
                } else {
                    ext = "jpg";
                }
                if (jQuery.inArray(ext, settings.knownFileTypes) > -1) {
                    $(this).wrapInner('<span class="file-text"></span>').prepend('<span class="file-icon"></span>').addClass(ext).addClass('file-icon');
                }
            });
        },
        destroy: function () {
            return this.each(function () {});
        }
    };
    $.fn.linktype = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.linktype');
        }
    };
})(jQuery);