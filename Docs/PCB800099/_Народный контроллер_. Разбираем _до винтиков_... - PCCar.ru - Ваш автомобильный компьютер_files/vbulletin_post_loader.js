/*======================================================================*\
|| #################################################################### ||
|| # vBulletin 3.8.4 Patch Level 1
|| # ---------------------------------------------------------------- # ||
|| # Copyright �2000-2009 Jelsoft Enterprises Ltd. All Rights Reserved. ||
|| # This file may not be redistributed in whole or significant part. # ||
|| # ---------------- VBULLETIN IS NOT FREE SOFTWARE ---------------- # ||
|| # http://www.vbulletin.com | http://www.vbulletin.com/license.html # ||
|| #################################################################### ||
\*======================================================================*/
function display_post(A){if(AJAX_Compatible){vB_PostLoader[A]=new vB_AJAX_PostLoader(A);vB_PostLoader[A].init()}else{pc_obj=fetch_object("postcount"+this.postid);openWindow("showpost.php?"+(SESSIONURL?"s="+SESSIONURL:"")+(pc_obj!=null?"&postcount="+PHP.urlencode(pc_obj.name):"")+"&p="+A)}return false}var vB_PostLoader=new Array();function vB_AJAX_PostLoader(A){this.postid=A;this.container=fetch_object("edit"+this.postid)}vB_AJAX_PostLoader.prototype.init=function(){if(this.container){postid=this.postid;pc_obj=fetch_object("postcount"+this.postid);YAHOO.util.Connect.asyncRequest("POST","showpost.php?p="+this.postid,{success:this.display,failure:this.handle_ajax_error,timeout:vB_Default_Timeout,scope:this},SESSIONURL+"securitytoken="+SECURITYTOKEN+"&ajax=1&postid="+this.postid+(pc_obj!=null?"&postcount="+PHP.urlencode(pc_obj.name):""))}};vB_AJAX_PostLoader.prototype.handle_ajax_error=function(A){vBulletin_AJAX_Error_Handler(A)};vB_AJAX_PostLoader.prototype.display=function(A){if(A.responseXML){var B=A.responseXML.getElementsByTagName("postbit");if(B.length){this.container.innerHTML=B[0].firstChild.nodeValue;PostBit_Init(fetch_object("post"+this.postid),this.postid)}else{openWindow("showpost.php?"+(SESSIONURL?"s="+SESSIONURL:"")+(pc_obj!=null?"&postcount="+PHP.urlencode(pc_obj.name):"")+"&p="+this.postid)}}};