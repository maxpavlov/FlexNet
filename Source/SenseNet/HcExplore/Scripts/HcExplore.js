$(document).ready(function(){

    /* Making treeview on the navigation menu with jQuery TreeView plugin */
    $(".treeview").treeview({
        animated: "fast",
        collapsed: true,
        unique: true
    });
    
	/* Hide empty divs */
    if ($(".hcexp-menu").find(".treeview")[0] == null) {
        $(".hcexp-menu").hide();
    }
    
    $(".hcexp-content").each(function(){
        if ($(this).find(".hcexp-panel")[0] == null) {
            $(this).hide();
        }
    });
    
});
