module ServiceWorker

open Fable.Core
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.Import
open Fable.PowerPack.Fetch
let [<Global>] self : Browser.ServiceWorker = jsNative
let CACHE_NAME = "Fable-PWA-Cache-v3"

let resources = [|
    "/js/bundle.js"
    "/Images/safe_favicon.png"
    "index.html"
    "manifest.json"
    "/"
|]

self.addEventListener_install
    (fun evt ->
        Browser.console.log("Installing")

        evt.waitUntil( promise {
            Browser.console.log("Before cache")
            let! cache = Browser.caches.``open`` CACHE_NAME
            Browser.console.log("After cache")
            do! cache.addAll !!resources
            Browser.console.log("After addAll")
            return None }))

self.addEventListener_activate(fun evt ->
        Browser.console.log("Activating")
        evt.waitUntil( promise {
            let! keys = Browser.caches.keys()
            let promises : JS.Iterable<U2<_,JS.PromiseLike<bool>>> = !!keys.map(fun k _ _-> if k <> CACHE_NAME then Browser.console.log("[ServiceWorker] Removing old cache ", k); Browser.caches.delete(k) else promise{return true})
            let! _ = JS.Promise.all promises
            return None }))

self.addEventListener_fetch(fun event ->
        Browser.console.log("Fetching")
        let req = event.request
        let url = req.url |> Browser.URL.Create
        Browser.console.log("METHOD: ", event.request.method ," and URL: ",url);
        event.respondWith(!^(promise{
            let! res = Browser.caches.``match``(req)
            Browser.console.log("Request: ", req);
            if isNull res then
                let! resp = fetch req.url []
                if not <| url.pathname.StartsWith("/api/") then
                    Browser.console.log("Adding ", req, " to the cache")
                    let! cache = Browser.caches.``open`` CACHE_NAME
                    do! cache.put(!!req, !!(resp?clone()))
                return !!resp
            else
                return res
        })))