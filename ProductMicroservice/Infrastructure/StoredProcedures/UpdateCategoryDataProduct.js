﻿function UpdateCategoryDataProduct(categoryPayload, continuationPayload) {
    function setResponse(data, status = 200, message = "OK") {
        getContext().getResponse().setBody({ status, message, data });
    }

    var collection = getContext().getCollection();
    var updatedItems = 0;

    var category = JSON.parse(categoryPayload);
    var continuation = JSON.parse(continuationPayload);

    if (continuation) {
        if (!continuation.lastCount || !continuation.token) {
            throw new Error("Invalid parameter")
        }

        updatedItems = continuation.lastCount;
        doUpdate(continuation.token);
    }
    else {
        doUpdate();
    }

    function doUpdate(continuationToken) {
        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(),
            `SELECT * FROM c WHERE c.deleted = false AND c.category.id = '${category.id}'`,
            { continuation: continuationToken },
            function (err, feed, options) {
                if (err) throw err;

                if (feed) {
                    for (var i = 0; i < feed.length; i++) {
                        feed[i].category.name = category.name;

                        collection.replaceDocument(feed[i]._self, feed[i], function (err, itemReplaced) {
                            if (err) throw err;
                        })

                        updatedItems += 1;
                    }
                }

                if (options.continuation) {
                    doUpdate(options.continuation);
                } else {
                    setResponse({ updatedItems, continuation: null });
                }
            }
        )

        if (!isAccepted) {
            setResponse({
                updatedItems: null,
                continuation: {
                    lastCount: updatedItems,
                    token: continuationToken
                }
            })
        }
    }
}