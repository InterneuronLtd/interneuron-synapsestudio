//var GlobalServiceURL = "http://dynamicapi.interneuron.io/";
//var GlobalServiceURL = "http://localhost:50769/";
//var GlobalServiceURL = 'https://interneuron.rnoh.nhs.uk/synapsedynamicapi/';
//var GlobalServiceURL = 'https://localhost:44374/';
var GlobalServiceURL = 'https://synapsedynamicapi.azurewebsites.net/';

var config = {

    //configure authority and client

    authority: "https://synapseidentityserver.azurewebsites.net",
    client_id: "SynapseStudio",
    redirect_uri: window.location.origin + "/callback.aspx",
    post_logout_redirect_uri: window.location.origin + "/logout.aspx?oidccallback=true",

    response_type: "id_token token",
    scope: "openid dynamicapi.read",

    //set default authentication provider for this client
    //acr_values: "idp:ADFS",

    //load userinfo from user info end point
    //disabled as SIS will copy userinfo into access token
    loadUserInfo: false,

    // This will get a new access_token via an iframe 60 secs before the old token is going to expire
    automaticSilentRenew: true,
    silent_redirect_uri: window.location.origin + "/SilentRenew.aspx",

    // will revoke access tokens at logout time
    revokeAccessTokenOnSignout: true,

    filterProtocolClaims: false,
    accessTokenExpiringNotificationTime: 60

};

