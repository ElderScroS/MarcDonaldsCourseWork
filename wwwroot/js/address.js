$(document).ready(function () {
    const $modalOverlay = $(".modal-overlay-new");
    const $closeModalBtn = $(".close-btn-new");
    const $addAddressBtn = $(".add-address-new");
    const $editAddressBtn = $(".edit-btn-new"); 
    const $backBtn = $(".back-btn-new");
    const $submitBtn = $(".submit-btn");
    const $modalContainer = $(".modal-container-relative");
    const $modalTitle = $('.modal-title-dynamic');
    const $currentPage = $('#currentPage');
    let selectedLocationType = "";

    const $addressListView = $("#address-list");
    const $addAddressFormView = $("#add-address");
    const $whatLocationFormView = $("#name-location");
    const $addressDetailsView = $("#address-details");
    const $addressEditView = $("#address-edit");

    const $addressInput = $("#address");
    const $latitudeInput = $("#latitude");
    const $longitudeInput = $("#longitude");
    const $form = $(".address-form");

    const $entranceInput = $("#entrance");
    const $errorLabel = $(".error-label");
    const $entranceInputEdit = $("#entrance-edit");
    const $floorApartmentEdit = $("#floor-apartment-edit")
    const $commentEdit = $("#comment-edit")
    const $errorLabelEdit = $(".error-label-edit");
    const $saveBtn = $("#save-btn");
    const $saveEditBtn = $("#save-btn-edit");

    let modalHistory = [];
    let hasBlurredEntrance = false;

    function resetModal() {
        $(".modal-view").removeClass("active slide-in slide-out");
        modalHistory = [];

        $addressListView.addClass("active slide-in");
        $backBtn.hide();

        $addressInput.val("");
        $latitudeInput.val("");
        $longitudeInput.val("");
        $submitBtn.prop("disabled", true).addClass("disabled");

        $entranceInput.val("");
        $errorLabel.hide();
        $saveBtn.prop("disabled", true).css("opacity", "0.7");
        hasBlurredEntrance = false;
    }

    function openModal() {
        resetModal();
        $modalOverlay.css("display", "flex").removeClass("hide").addClass("show");
        $("body").addClass("modal-open");
    }

    function closeModal() {
        $modalOverlay.removeClass("show").addClass("hide");
        setTimeout(() => {
            $modalOverlay.css("display", "none");
            $("body").removeClass("modal-open");
            resetModal();
        }, 300);
    }

    function switchToForm() {
        $addressListView.removeClass("active slide-in").addClass("slide-out");
        $addAddressFormView.removeClass("slide-out").addClass("slide-in active");
        $backBtn.show();
        modalHistory.push("address-list");
    }

    function switchToLocationType() {
        $addAddressFormView.removeClass("active slide-in").addClass("slide-out");
        $whatLocationFormView.removeClass("slide-out").addClass("slide-in active");
        $backBtn.show();
        modalHistory.push("address-location");
    }

    function switchToAddressDetails() {
        $whatLocationFormView.removeClass("active slide-in").addClass("slide-out");
        $addressDetailsView.removeClass("slide-out").addClass("slide-in active");

        $backBtn.show();
        modalHistory.push("address-details");
    }

    function switchToAddressEdit() {
        $addressListView.removeClass("active slide-in").addClass("slide-out");
        $addressEditView.removeClass("slide-out").addClass("slide-in active");
        $backBtn.show();

        const addressId = $(this).data('address-id');
        const addressEntrance = $(this).data('address-entrance');
        const addressFloorApartment = $(this).data('address-floor');
        const addressComment = $(this).data('address-comment');
        const latitude = parseFloat($(this).data('latitude'));
        const longitude = parseFloat($(this).data('longitude'));

        $saveEditBtn.attr('data-address-id', addressId);
        $entranceInputEdit.val(addressEntrance);
        $floorApartmentEdit.val(addressFloorApartment);
        $commentEdit.val(addressComment);

        if ($entranceInputEdit.val().trim() !== "") {
            $errorLabel.hide();
            $saveEditBtn.prop("disabled", false).css("opacity", "1");
        }

        initMapEdit(latitude, longitude);
        
        modalHistory.push("address-edit");
    }

    $backBtn.on("click", function () {
        if (modalHistory.length === 0) return;

        const previous = modalHistory.pop();
        $(".modal-view").removeClass("active slide-in slide-out");

        if (previous === "address-edit") {
            $addressListView.addClass("active slide-in");
            modalHistory = []; 
            $backBtn.hide();
            return;
        }

        if (previous === "address-list") {
            $addressListView.addClass("active slide-in");
        } else if (previous === "address-location") {
            $addAddressFormView.addClass("active slide-in");
        } else if (previous === "address-details") {
            $whatLocationFormView.addClass("active slide-in");
        }

        if (modalHistory.length === 0) {
            $backBtn.hide();
        }
    });

    window.selectLocationType = function (type) {
        document.getElementById('name-location').classList.remove('active');
        document.getElementById('address-details').classList.add('active');
        selectedLocationType = type;
        switchToAddressDetails();
    };

    $form.on("submit", function (e) {
        e.preventDefault();
        switchToLocationType();
    });

    $modalContainer.on('scroll', function () {
        const scrollTop = $modalContainer.scrollTop();

        if (scrollTop > 10) {
            $('.modal-header-new').addClass('scrolled');
    
            if ($('#address-list').is(':visible')) {
                $modalTitle.text('Address list');
            } else if ($('#address-details').is(':visible')) {
                $modalTitle.text('Address details');
            } else if ($('#address-edit').is(':visible')) {
                $modalTitle.text('Edit address');
            }
        } else {
            $('.modal-header-new').removeClass('scrolled');
            $modalTitle.text('');
        }
    });

    window.getLocation = function () {
        const $iconButton = $(".icon-button");
        $iconButton.addClass("loading");
    
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(async function (position) {
                const lat = position.coords.latitude;
                const lon = position.coords.longitude;
    
                $latitudeInput.val(lat);
                $longitudeInput.val(lon);
    
                try {
                    const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`);
                    const data = await res.json();
    
                    if (data.address) {
                        let street = data.address.road || "";
                        const house = data.address.house_number || "";
                        const cityRaw = data.address.city || data.address.town || data.address.village || "";
                        const country = data.address.country || "";

                        street = street.replace(/улица|Улица/i, "").trim();
                        const city = cityRaw.includes("Бак") ? "Баку" : cityRaw;
    
                        const fullAddress = `${street} ${house}, ${city}, ${country}`.trim();
    
                        $(".street").text(street);
                        $(".house").text(house);
                        $(".city").text(city);
                        $(".country").text(country);
    
                        $addressInput.val(fullAddress);
                    }
    
                    checkAddressFilled();
                    $iconButton.removeClass("loading");

                    const latInit = parseFloat($latitudeInput.val());
                    const lonInit = parseFloat($longitudeInput.val());

                    setTimeout(() => {
                        initMap(latInit, lonInit);
                    }, 300);
                } catch (err) {
                    alert("Ошибка при определении адреса.");
                    $iconButton.removeClass("loading");
                }
            }, function (error) {
                alert("Не удалось определить местоположение: " + error.message);
                $iconButton.removeClass("loading");
            });
        } else {
            alert("Ваш браузер не поддерживает геолокацию.");
            $iconButton.removeClass("loading");
        }
    };    
    
    function checkAddressFilled() {
        const value = $addressInput.val().trim();
        if (value.length > 7) {
            $submitBtn.prop("disabled", false).removeClass("disabled");
        } else {
            $submitBtn.prop("disabled", true).addClass("disabled");
        }
    }

    function initMap(lat, lon) {
        mapboxgl.accessToken = 'pk.eyJ1IjoibWFyazQ1IiwiYSI6ImNtYWh2OXBmaTBlazMyanNpaWwxeG96aGQifQ.EfZspWA49HiUZHOxsZ0dWA';

        const map = new mapboxgl.Map({
            container: 'map',
            style: 'mapbox://styles/mapbox/dark-v10',
            center: [lon, lat],
            zoom: 13
        });
        map.on('load', () => {
            map.resize();
        });

        new mapboxgl.Marker()
            .setLngLat([lon, lat])
            .addTo(map);

        const boundsd = new mapboxgl.LngLatBounds();
        boundsd.extend([lon, lat]);

        map.fitBounds(boundsd, {
            padding: 100,
            maxZoom: 16,
            duration: 1000
        });
    }
    function initMapEdit(lat, lon) {
        mapboxgl.accessToken = 'pk.eyJ1IjoibWFyazQ1IiwiYSI6ImNtYWh2OXBmaTBlazMyanNpaWwxeG96aGQifQ.EfZspWA49HiUZHOxsZ0dWA';

        const mapEdit = new mapboxgl.Map({
            container: 'map-edit',
            style: 'mapbox://styles/mapbox/dark-v10',
            center: [lon, lat],
            zoom: 13
        });

        new mapboxgl.Marker()
            .setLngLat([lon, lat])
            .addTo(mapEdit);

        const bounds = new mapboxgl.LngLatBounds();
        bounds.extend([lon, lat]);

        mapEdit.fitBounds(bounds, {
            padding: 100,
            maxZoom: 16,
            duration: 1000
        });
    }

    $("#addressBtn").on("click", openModal);
    $closeModalBtn.on("click", closeModal);
    $modalOverlay.on("click", function (e) {
        if (e.target === this) closeModal();
    });
    $addAddressBtn.on("click", switchToForm);
    $editAddressBtn.on("click", switchToAddressEdit);

    $entranceInput.on("blur", function () {
        const val = $entranceInput.val().trim();

        if (!hasBlurredEntrance) {
            hasBlurredEntrance = true;
            if (val === "") {
                $errorLabel.show();
                $saveBtn.prop("disabled", true).css("opacity", "0.7");
            }
        } else {
            if (val === "") {
                $errorLabel.show();
                $saveBtn.prop("disabled", true).css("opacity", "0.7");
            }
        }
    });
    $entranceInput.on("input", function () {
        const val = $entranceInput.val().trim();

        if (val !== "") {
            $errorLabel.hide();
            $saveBtn.prop("disabled", false).css("opacity", "1");
        } else if (hasBlurredEntrance) {
            $errorLabel.show();
            $saveBtn.prop("disabled", true).css("opacity", "0.7");
        }
    });

    $entranceInputEdit.on("blur", function () {
        const val = $entranceInputEdit.val().trim();

        if (!hasBlurredEntrance) {
            hasBlurredEntrance = true;
            if (val === "") {
                $errorLabelEdit.show();
                $saveEditBtn.prop("disabled", true).css("opacity", "0.7");
            }
        } else {
            if (val === "") {
                $errorLabelEdit.show();
                $saveEditBtn.prop("disabled", true).css("opacity", "0.7");
            }
        }
    });
    $entranceInputEdit.on("input", function () {
        const val = $entranceInputEdit.val().trim();

        if (val !== "") {
            $errorLabel.hide();
            $saveEditBtn.prop("disabled", false).css("opacity", "1");
        } else if (hasBlurredEntrance) {
            $errorLabelEdit.show();
            $saveEditBtn.prop("disabled", true).css("opacity", "0.7");
        }
    });

    $saveBtn.on("click", function () {
        const address = {
            Title: selectedLocationType,
            City: $(".city").text().trim(),
            Street: $(".street").text().trim(),
            HouseNumber: $(".house").text().trim(),
            Entrance: $("#entrance").val().trim(),
            FloorApartment: $("#floor-apartment").val().trim(),
            Comment: $("#comment").val()?.trim(),
            Latitude: parseFloat($latitudeInput.val()),
            Longitude: parseFloat($longitudeInput.val()),
        };
    
        $.ajax({
            url: "/User/AddAddress",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(address),
            success: function (response) {
                closeModal()

                const currentPageText = $currentPage.text().trim();

                if (currentPageText === "Profile") {
                    window.location.href = "/User/Profile";
                }
                else {
                    window.location.href = "/User/Cart";
                }
            },
            error: function () {
            }
        });
    });
    $saveEditBtn.on("click", function () {
        const address = {
            Id: $saveEditBtn.data('address-id'),
            Entrance: $entranceInputEdit.val().trim(),
            FloorApartment: $floorApartmentEdit.val()?.trim(),
            Comment: $commentEdit.val()?.trim(),
        };

        $.ajax({
            url: "/User/EditAddress",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(address),
            success: function (response) {
                $addressListView.removeClass("slide-out").addClass("active slide-in");
                $addressEditView.removeClass("active slide-in");

                $editAddressBtn.data('address-entrance', address.Entrance)
                    .data('address-floor', address.FloorApartment)
                    .data('address-comment', address.Comment);

                $backBtn.hide();
            },
            error: function () {
            }
        });
    });
});
