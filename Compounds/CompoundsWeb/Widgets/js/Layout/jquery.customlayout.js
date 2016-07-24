//@ sourceURL=jquery.customlayout.js

$(function () {
    $.widget("layout.customlayout", $.layout.layoutmanager, {

        options: {
            type: 'vertical',
            border: 'between'
        },

        _onBeforeInit: function() {
        	this.options.config = {
    		  	layout: {
    		  		layoutType: 'vertical',
    		  		items: [{
		  		        id: 'top',
		  		        size: '70%',
    		  			view: {
                            name: 'areastats'
                        }		  			
    		  		},
    		  		{
    		  			id: 'bottom',
    		  			size: '30%',
    		  			view: {
                            name: 'areastats'
                        }		  			
    		  		}],			    
    			}
    		};
        },
    });

}(jQuery));
