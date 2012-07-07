<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>
<script>
    $(window).load(function ()
    {
        $.getScript("http://www.sensenet.com/Root/Sites/Default_Site/version.js?callback=?", function ()
        {
            
            var name = (SenseNetVersion.name);
            var versionnumber = name + " " + (SenseNetVersion.versionnumber);
            var edition = (SenseNetVersion.edition);
            var releasedate = (SenseNetVersion.releasedate);
            var changeloglink = (SenseNetVersion.changeloglink);
            var downloadlink = (SenseNetVersion.downloadlink);
            var status = (SenseNetVersion.status);

            var currentVersion = $(".sn-copyright a:last").text();
            var substrCurrentVersion = currentVersion.split('(')[0];
            currentVersionnum = substrCurrentVersion;

            latestVersionnum = versionnumber;


            if (currentVersionnum == null)
            {
                $('.info').hide();
                $('.arrow').hide();
            }

            if (currentVersionnum == 60)
            {
                $('.info').hide();
                $('.arrow').hide();
            }
            else if (latestVersionnum < currentVersionnum)
            {
                $('.info').hide();
                $('.arrow').hide();
            }
            else if (latestVersionnum > currentVersionnum)
            {
                $('.info').html('<h1>Available new versions</h1><ul><li><b>' + name + ' ' + versionnumber + ' ' + edition + '</b></li><li>' + releasedate + ' - ' + status + '</li><li class="last"><a href="">changelog</a>  |  <a href="">download</a></li></ul>').css('display', 'block');
                $('.arrow').css('display', 'block');
                $('.info .last a:first').attr('href', changeloglink);
                $('.info .last a:last').attr('href', downloadlink);
            }
            else
            {
                $('.versionInfo').addClass('latest');
                $('.info').html('This is the latest version of Sense/Net ECM Community Edition').css('display', 'block');
                $('.arrow').css('display', 'block');
            }
        });


    });
    
    

    
    

</script>
<style>
.versiondiv{position: absolute;top: 5px;left: 33%;}
.versionInfo {color: #fff;z-index:1000000}
/*.ie9 .versionInfo{width: 254px;}*/
.versionInfo .arrow {background:url(/Root/Global/images/arrow.png) top right no-repeat;width:20px;height:20px;margin-left:10px;float: left;margin-top: 15px;}
.versionInfo .info {background: #f15a24;padding: 10px;border: solid 1px #f15a24;-webkit-border-radius: 5px;-moz-border-radius: 5px;border-radius: 5px;min-height: 20px;float: right;}
.versionInfo .info h1 {color: #fff;font-size: 14px;margin: 0px 0px 5px 0px;border-bottom: solid 1px #fff;padding-bottom: 3px;}
.versionInfo .info ul {margin: 0;padding: 0;}
.versionInfo .info ul li {color: #fff;list-style-type: none;margin-bottom: 5px;padding: 0;}
.versionInfo .info ul li.links {margin-bottom: 0;}
.versionInfo a {color: #fff;text-decoration: underline;font-weight: bold;}
.versionInfo.latest .arrow {background:url(/Root/Global/images/arrow_blue.png) top right no-repeat;}
.versionInfo.latest .info {background:#64A1CB;border-color: #64A1CB;float: right;
width: 196px;}
</style>

    <div class="sn-hide"></div>
    <div class="arrow" style="display:none"></div>
    <div class="info" style="display:none"></div>

