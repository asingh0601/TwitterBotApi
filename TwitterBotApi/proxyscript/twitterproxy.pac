function FindProxyForURL(url, host) {
    if (shExpMatch(url,"*.x.com*")) {
        return "PROXY 103.172.84.210:59100";
    }else if (shExpMatch(url,"*.whatismyipaddress.com*")) {
        return "PROXY 103.172.84.210:59100";
    } else {
        return "DIRECT";
    }
}