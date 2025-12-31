app.controller("MangaLabController", function ($scope, $location, MangaLabService, $window, $timeout, $q) { 

    //LOGIN
    //REGISTRATION
    //EDIT USER
    //USER STATISTICS
    //STATISTICS(CHARTS & TABLES) PAGE
    //REDIRECTIONS
    //SEARCH PAGE
    //MANGALIST PAGE
    //RECENTLY ADDED
    //MANGA / DETAIL PAGE
    //READ PAGE

    //#################################################### LOGIN ####################################################

    //Session Integrity
    $scope.isLoggedIn = false;
    $scope.isAdmin = false;
    
    $scope.loginData = {
        User: '',
        Password: ''
    };
    // Message from csharp controller
    $scope.loginError = '';
    $scope.isSubmitting = false;

    // Main Func to handle logging in
    $scope.loginUser = function () {
        $scope.loginError = '';
        if ($scope.signIn.$invalid) return;
        $scope.isSubmitting = true;
        MangaLabService.login($scope.loginData).then(function (response) {
            $scope.isSubmitting = false;
            var msg = response.data.message;
            if (msg === "success") {
                $scope.redirectToHome();
            } else if (msg === "Invalid username or password") {
                $scope.loginError = "Invalid username or password.";
            } else {
                $scope.loginError = msg;
            }
        })
    };

    // Function Loader
    // Gets the necessary session from the dbase
    // Nothing Confidential
    function LoginStatus() {

        MangaLabService.getLoginStatus().then(function (response) {

            var sessionData = response.data;
            if (sessionData && sessionData.isLoggedIn) {

                //Gets Role and Profile Pic
                $scope.userrole = sessionData.role;
                $scope.userpic = sessionData.profilePic; 

                //Logged In
                $scope.isLoggedIn = true;

                // Administration Page Function Loader
                if ($scope.userrole && $scope.userrole.trim().toLowerCase() === "admin") {
                    $scope.isAdmin = true;
                    $scope.loadAdminCharts();
                    $scope.loadGetLogsList();
                    $scope.loadTables();
                }

               
                $scope.loadFavorites();
                $scope.loadContinueReading();
                $scope.loadAccountStats();
                
                

            } else {
                $scope.username = '';
                $scope.userrole = '';
                $scope.userpic = '';
                $scope.isLoggedIn = false;
                $scope.isAdmin = false;
            }
        })
    }

    LoginStatus();

    //Logout (Destroy the session rahh)
    $scope.logout = function () {
        MangaLabService.logout().then(function (response) {
            if (response.data.success) {
                $scope.username = '';
                $scope.userrole = '';
                $scope.userpic = '';
                $scope.isLoggedIn = false;
                $scope.isAdmin = false;
                $scope.redirectToHome();
            } 
        });
    };

    //#################################################### LOGIN ####################################################







    //#################################################### REGISTRATION ####################################################

    //ShowPassword
    $scope.showPassword = false;
    $scope.showConfirm = false;

    //ShowPassword for acc
    $scope.showEmailPass = false;
    $scope.showCurrentPass = false;
    $scope.showNewPass = false;
    $scope.showConfirmPass = false;

    //Resuable Toggler
    $scope.toggle = function (field) {
        $scope[field] = !$scope[field];
    };

    //Patterns
    $scope.email = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
    $scope.password = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[~`!@#$%^&*()_\-+={}[\]\|\\:;"'<>,.?/]).{8,}$/;

    //Registration
    $scope.user = {
        username: '',
        email: '',
        password: '',
        confirmPassword: '' 
    };
    //Loading Spinner
    $scope.isSubmitting = false;
    //addUser == Registration Function
    //from MP(ewan ko na)
    $scope.addUser = function () {
        $scope.usernameChecker = false;
        $scope.emailChecker = false;
        const addedUser = $scope.user;

        $scope.isSubmitting = true;

        MangaLabService.saveRegis(addedUser).then(function (response) {
            var msg = response.data.message;

            if (msg === "usernameTaken") {
                $scope.usernameChecker = true;
                $scope.isSubmitting = false;
                return;
            }

            if (msg === "emailTaken") {
                $scope.emailChecker = true;
                $scope.isSubmitting = false;
                return;
            }

            if (msg === "success") {
                $scope.user = {
                    username: '',
                    email: '',
                    password: '',
                    confirmPassword: ''
                };

                $scope.redirectToSignIn();

                if ($scope.signUp) {
                    $scope.signUp.$setPristine();
                    $scope.signUp.$setUntouched();
                }
            }
        });

    };

    //#################################################### REGISTRATION ####################################################




    //#################################################### EDIT USER ####################################################

    //Editing Component ng-show
    $scope.showEdit = false;
    $scope.showEditUsername = true;

    //Btns
    $scope.toggleEditUsername = function () {
        $scope.showEditUsername = true;
    }
    $scope.toggleEditSecurity = function () {
        $scope.showEditUsername = false;
    }

    //Initialize
    $scope.newProfilePic = {
        file: null
    };

    $scope.accountUser = {
        username: "" 
    };

    $scope.originalUserpic = "";

    //Form Shower
    $scope.editUser = function () {
        $scope.showEdit = true;
        $scope.showEditUsername = true;
        $scope.fileToUpload = null;

        //
        var inputElement = document.getElementById('username');
        if (inputElement) {
            $scope.accountUser.username = inputElement.value;
        }
    };

    $scope.cancelEdit = function () {
        $scope.showEdit = false;
    }

    //Validation
    $scope.resetUsernameError = function () {
        $scope.changeUsernameChecker = false;
    };


    $scope.saveEdit = function () {

        $scope.changeUsernameChecker = false;

        //minlength
        //matic redirect sa validation error
        if ($scope.editProfileForm.$invalid) {
            return;
        }

        var fileInput = document.getElementById('profilePicInput');
        var file = fileInput.files[0];

        var newUsername = $scope.accountUser.username;

        var formData = new FormData();
        formData.append("newUsername", newUsername);
        if (file) {
            formData.append("profilePicFile", file);
        }

        MangaLabService.saveEdit(formData).then(function (returnedData) {

            if (returnedData.data.success) {

                fileInput.value = null;

                //Band-aid solution
                //reload nalang ung page to see the new pfp and username
                $timeout(function () {
                    $window.location.reload();
                }, 1500);

            } else {
                if (returnedData.data.message === "Username exists.") {

                    $scope.changeUsernameChecker = true;

                } else {
                    console.error("Error:", returnedData.data.message);
                }
            }
        })
            
    };


    $scope.sec = {
        email: {
            newEmail: "",
            currentPassword: ""
        },
        pw: {
            currentPassword: "",
            newPassword: "",
            confirmNewPassword: ""
        }
    };


    $scope.emailSuccess = null;
    $scope.emailError = null;
    $scope.isEmailUpdating = false; 

    $scope.updateEmail = function () {

        if ($scope.changeEmailForm.$invalid) {
            return;
        }

        $scope.isEmailUpdating = true;
        $scope.emailSuccess = null;
        $scope.emailError = null;
        $scope.emailChecker = false; 


        MangaLabService.updateEmail($scope.sec.email.currentPassword,$scope.sec.email.newEmail).then(function (response) {
                
                $scope.isEmailUpdating = false;

                if (response.data.success) {
                    $scope.emailSuccess = response.data.message; 
                    $scope.sec.email = {};
                    $scope.changeEmailForm.$setPristine();
                    $scope.changeEmailForm.$setUntouched();

                    $timeout(function () {
                        $scope.emailSuccess = null;
                    }, 5000);
                    

                } else {
                    if (response.data.message && response.data.message.toLowerCase().includes("email already exists")) {
                        $scope.emailChecker = true; 
                    } else {
                        $scope.emailError = response.data.message;
                    }
                }
            });
    };

    $scope.passwordSuccess = null;
    $scope.passwordError = null;
    $scope.isPasswordUpdating = false;

    $scope.updatePassword = function () {
        if ($scope.changePasswordForm.$invalid || $scope.sec.pw.newPassword !== $scope.sec.pw.confirmNewPassword) {
            return;
        }

        $scope.isPasswordUpdating = true;
        $scope.passwordSuccess = null;
        $scope.passwordError = null;


        MangaLabService.updatePassword($scope.sec.pw.currentPassword,$scope.sec.pw.newPassword,$scope.sec.pw.confirmNewPassword)
        .then(function (response) {
            $scope.isPasswordUpdating = false;

            if (response.data.success) {
                $scope.passwordSuccess = response.data.message;
                $scope.sec.pw = {};
                $scope.changePasswordForm.$setPristine();
                $scope.changePasswordForm.$setUntouched();


                $timeout(function () {
                    $scope.passwordSuccess = null;
                }, 5000);

                
            } else {
                $scope.passwordError = response.data.message;
            }
        });
    };


    //#################################################### EDIT USER ####################################################








    //#################################################### USER STATISTICS ####################################################

    //User Statistics
    $scope.currentMangaID = null;
    $scope.isFavorite = false;

    $scope.favoriteManga = [];
    $scope.favoriteGenre = "Loading...";



    $scope.addFavorite = function () {
        var mangaID = $scope.currentMangaID;

        MangaLabService.addFavorite(mangaID).then(function (response) {
            if (response.data.message === "success") {
                $scope.isFavorite = true;
            }
        });
    };

    $scope.unFavoriteManga = function () {
        var mangaID = $scope.currentMangaID;

        MangaLabService.archiveFavoriteByMangaID(mangaID).then(function (response) {
            if (response.data.success) {
                $scope.isFavorite = false;
            } else {
                console.error("Failed to unfavorite:", response.data.message);
            }
        });
    };

    function checkFavoriteStatus() {
        var mangaID = $scope.currentMangaID;

        MangaLabService.isFavorite(mangaID).then(function (response) {
            $scope.isFavorite = response.data.isFavorite;
        });
    }



    $scope.archiveFavoriteItem = function (itemToArchive) {
        if (!itemToArchive) return;

        MangaLabService.archiveFavoriteItem(itemToArchive.favoriteID)
            .then(function (response) {
                if (response.data.success) {
                    $scope.favoriteManga = $scope.favoriteManga.filter(function (item) {
                        return item.favoriteID !== itemToArchive.favoriteID;
                    });
                } else {
                    console.error("Failed to archive favorite:", response.data.message);
                }
            })
            .catch(function (error) {
                console.error("Error", error);
            });
    };

    $scope.loadFavorites = function () {

            MangaLabService.getFavoriteManga().then(function (response) {

                $scope.favoriteManga = response.data; 

                if (!$scope.favoriteManga || $scope.favoriteManga.length === 0) {
                    $scope.favoriteGenre = "N/A";
                    return;
                }

                var genreCounts = {};
                var genreIdMap = {};

                $scope.favoriteManga.forEach(function (item) {
                    if (item.manga) { 
                        item.manga.attributes.tags.forEach(function (tag) {
                            if (tag.attributes.group === 'genre') {
                                var genre = tag.attributes.name.en;
                                var genreId = tag.id;
                                if (genre) {
                                    genreCounts[genre] = (genreCounts[genre] || 0) + 1;
                                    genreIdMap[genre] = genreId;
                                }
                            }
                        });
                    }
                });

                var maxCount = 0;
                var topGenre = "N/A";
                for (var genreName in genreCounts) {
                    if (genreCounts[genreName] > maxCount) {
                        maxCount = genreCounts[genreName];
                        topGenre = genreName;
                    }
                }

                $scope.favoriteGenre = topGenre;

                if (topGenre !== "N/A") {
                    var topGenreId = genreIdMap[topGenre];
                    $scope.getRecommendations(topGenreId);
                }
            })
        
    };


    
    $scope.recommendedManga = [];

    $scope.getRecommendations = function (genreId) {

        if (!genreId) {
            $scope.recommendedManga = []; 
            return;
        }

        MangaLabService.getRecommendedManga(genreId)
            .then(function (response) {

                $scope.recommendedManga = response.data.data;

                if (!$scope.recommendedManga) {
                    $scope.recommendedManga = [];
                }
            })
            .catch(function (error) {
                console.error("Error getting recommended manga:", error);
                $scope.recommendedManga = []; 
            });
    };

   

    $scope.getCoverUrl = function (manga) {
        if (!manga || !manga.relationships) return 'default-cover.png';

        var cover = manga.relationships.find(rel => rel.type === 'cover_art');

        if (cover && cover.attributes) {
            return `https://uploads.mangadex.org/covers/${manga.id}/${cover.attributes.fileName}.256.jpg`;
        }
        return 'default-cover.png'; 
    };


    $scope.archiveHistoryItem = function (itemToArchive) {
        if (!itemToArchive) return;

        MangaLabService.archiveHistoryItem(itemToArchive.historyID)
            .then(function (response) {
                if (response.data.success) {
                    
                    $scope.continueReading = $scope.continueReading.filter(function (item) {
                        return item.historyID !== itemToArchive.historyID;
                    });
                } else {
                    console.error("Failed to archive item:", response.data.message);
                    alert("Could not remove item: " + response.data.message);
                }
            })
            
    };

    $scope.getDisplayTitle = function (manga) {
        if (!manga || !manga.attributes || !manga.attributes.title) {
            return "Title Not Found";
        }

        var titles = manga.attributes.title;
        var originalLang = manga.attributes.originalLanguage;

        return titles.en ||
            titles['ja-ro'] ||
            titles.ja ||
            titles[originalLang] ||
            Object.values(titles)[0]; 
    };

    
    $scope.loadAccountStats = function () {
        MangaLabService.getAccountStats().then(function (response) {
            if (response.data.success) {
                $scope.accountAge = response.data.accountAge;
                $scope.totalMangasRead = response.data.totalMangasRead;
            } else {
                console.error("Could not load account stats:", response.data.message);
                $scope.accountAge = "N/A";
                $scope.totalMangasRead = "N/A";
            }
        }).catch(function (err) {
            console.error("Error fetching account stats:", err);
            $scope.accountAge = "Error";
            $scope.totalMangasRead = "Error";
        });
    };


    $scope.continueReading = [];
    $scope.loadContinueReading = function () {
        MangaLabService.getContinueReading().then(function (response) {
            $scope.continueReading = response.data;
        }).catch(function (err) {
            console.error("Error loading reading history:", err);
        });
    };


    //#################################################### USER STATISTICS ####################################################






    //#################################################### STATISTICS (CHARTS & TABLES) PAGE ####################################################


    

    // Generalized GET ng chart Data
    
    $scope.loadAdminCharts = function () {


        //Issues with the Download Analytics button
        // $q = buffer to check if lahat ng charts/data is loaded before maging clickable yung dl button
        // "Array of Promises"
        $scope.readyDLButton = false;

        var Check1 = MangaLabService.getTopFavoriteMangaStats().then(function (response) {
            if (response.data.success) {
                $scope.topMangaLabels = response.data.topMangaLabels;
                $scope.topMangaData = response.data.topMangaData;
                $scope.topGenreLabels = response.data.topGenreLabels;
                $scope.topGenreData = response.data.topGenreData;
            }
        });

        var Check2 = MangaLabService.getTopReadMangaStats().then(function (response) {
            if (response.data.success) {
                $scope.topReadLabels = response.data.labels;
                $scope.topReadData = response.data.data;
            }
        });

        var Check3 = MangaLabService.getNewUsers().then(function (response) {
            if (response.data.success) {
                $scope.newUsersLabels = response.data.labels;
                $scope.newUsersData = [response.data.data];
            }
        });

        var Check4 = MangaLabService.getDailyLoginStats().then(function (response) {
            if (response.data.success) {
                $scope.dailyLoginLabels = response.data.labels;
                $scope.dailyLoginData = [response.data.data];
            }
        });

        $q.all([Check1, Check2, Check3, Check4]).then(function () {
                $scope.readyDLButton = true;
         })
           
    };


    //Most Favorited Manga (Horizontal bar Chart)
    $scope.topMangaLabels = [];
    $scope.topMangaData = []; 

    $scope.topMangaOptions = {
        legend: { display: false }, 
        scales: {
            yAxes: [{
                ticks: { fontColor: 'white' }, 
                gridLines: { color: 'rgba(255, 255, 255, 0.2)' }
            }],
            xAxes: [{
                ticks: { fontColor: 'white', beginAtZero: true }, 
                gridLines: { color: 'rgba(255, 255, 255, 0.2)' }
            }]
        }
    };

    

    //Most Favorited Genre (Pie Chart)
    $scope.topGenreLabels = [];
    $scope.topGenreData = [];

    $scope.topGenreOptions = {
        legend: {
            display: false,
            labels: {
                fontColor: 'white' 
            }
        }
    };

    //Most Read Manga (Horizontal Bar Chart)
    $scope.topReadLabels = [];
    $scope.topReadData = [];
    $scope.topReadOptions = $scope.topMangaOptions;


    // New Users (Line Chart)
    $scope.newUsersLabels = [];
    $scope.newUsersData = [];
    $scope.newUsersSeries = ['New Users'];
    $scope.newUsersColors = ['#FF0000'];

    $scope.newUsersOptions = {
        scales: {
            y: {
                ticks: { color: 'white', beginAtZero: true },
                grid: { color: 'rgba(255,255,255,0.2)' }
            },
            x: {
                ticks: { color: 'white' },
                grid: { color: 'rgba(255,255,255,0.2)' }
            }
        }
    };


    // Daily Logins (Bar Chart)

    $scope.dailyLoginLabels = [];
    $scope.dailyLoginData = [];
    $scope.dailyLoginSeries = ['Logins']; 
    $scope.dailyLoginColors = ['#FF0000'];




    //Tables


    $scope.showAccounts = true;
    $scope.showArchived = false;
    $scope.showLogs = false;


    $scope.showAccountsView = function () {
        $scope.showAccounts = true;
        $scope.showArchived = false;
        $scope.showLogs = false;
    };

    $scope.showArchivedView = function () {
        $scope.showAccounts = false;
        $scope.showArchived = true;
        $scope.showLogs = false;
    };

    $scope.showLogsView = function () {
        $scope.showAccounts = false;
        $scope.showArchived = false;
        $scope.showLogs = true;
    };
    

    $scope.logsList = [];
    $scope.loadGetLogsList = function () {


        MangaLabService.getLogsList().then(function (response) {

            $scope.logsList = response.data.data;

            $timeout(function () {
                $('#logsTable').DataTable({
                    columnControl: ['order', ['orderAsc', 'orderDesc', 'search']],
                });

            }, 0);
        });
    };
    

    $scope.activeAccountList = [];
    $scope.archivedAccountList = [];

    $scope.tablesReady = true;

    $scope.loadTables = function () {
        var noCacheParams = { params: { _: new Date().getTime() } };
        return MangaLabService.getAccountsList(noCacheParams).then(function (response) {
            var result = response.data;
            if (result.success) {
                $scope.tablesReady = false;
                $scope.activeAccountList = result.activeAccounts;
                $scope.archivedAccountList = result.archivedAccounts;

                $timeout(function () {

                    $scope.tablesReady = true;
                    $timeout(function () {
                        $('#activeAccountTable').DataTable({
                            columnControl: ['order', ['orderAsc', 'orderDesc', 'search']],
                            order: [] 

                            
                        });

                        $('#archivedAccountTable').DataTable({
                            columnControl: ['order', ['orderAsc', 'orderDesc', 'search']],
                            order: [] 
                        });
                        

                    }, 100);

                }, 0);
            }
        });
    };
    


    $scope.availableRoles = [];

    // Load Roles then call
    // Since hindi static yung role loading, need kunin from tbl_roles
    // Load roles paloob sa modal
    $scope.loadRoles = function () {
        MangaLabService.getRoleList().then(function (response) {
            if (response.data.success) {
                $scope.availableRoles = response.data.data;
            }
        });
    };
    $scope.loadRoles(); 

    // click edit, open modal
    $scope.openRoleModal = function (item) {
        // Copy muna so yung mga changes sa loob ng modal dropdown is hindi makakaaffect sa table
        $scope.tempUser = angular.copy(item);

        //goto saveRoleChange func
        $scope.tempUser.selectedRoleID = item.roleID;

        //Open modal
        $('#roleModal').modal('show');
    };

    // save from modal
    $scope.saveRoleChange = function () {
        //get data from openRoleModal func
        var regID = $scope.tempUser.registrationID;
        var newRoleID = $scope.tempUser.selectedRoleID;

        MangaLabService.updateUserRole(regID, newRoleID).then(function (response) {
            if (response.data.success) {
                $('#roleModal').modal('hide');

                document.getElementById('toastMessage').innerText = response.data.message;
                new bootstrap.Toast(document.getElementById('successToast')).show();

                $scope.loadTables(); 
            } else {
                alert("Error: " + response.data.message);
            }
        });
    };


    $scope.changeStatus = function (registrationID) {

        var isActive = $scope.activeAccountList.some(item => item.registrationID === registrationID);
        var newStatus = isActive ? 1 : 0;  
        var action = isActive ? "archive" : "unarchive";

        var confirmationMessage = `Are you sure you want to ${action} the account with ID ${registrationID}?`;

        $('#confirmationBody').text(confirmationMessage);
        $('#confirmationModal').modal('show');

        $('#confirmActionButton').off('click').on('click', function () {

            $('#confirmationModal').modal('hide');

            MangaLabService.updateAccountStatus(registrationID, newStatus)
                .then(function (response) {
                    var result = response.data;

                    if (result && result.success) {

                        $('#toastMessage').text(result.message || `Account ${action}d successfully.`);
                        new bootstrap.Toast(document.getElementById('successToast')).show();

                        $scope.loadTables();

                    } else {
                        $('#modalTitle').text("Error");
                        $('#modalBody').text("Status Change Failed: " + result.message);
                        $('#statusModal').modal('show');
                    }
                })
               
        });
    };

    //PDF REPORT

    //Most Read Manga (Trial)
    //Get Manga title and data from
    //$scope.topReadLabels && $scope.topReadData
    //nagpopulate = loadAdminCharts function
    $scope.createMostReadTable = function () {

        //Header
        var body = [
            [
                { text: 'Most Read Manga', style: 'tableHeader', alignment: 'center' },
                { text: 'Read Count', style: 'tableHeader', alignment: 'center' }
            ]
        ];

        if ($scope.topReadLabels && $scope.topReadData) {

            for (var i = 0; i < $scope.topReadLabels.length; i++) {

                var row = [
                    {
                        //Manga Title
                        text: $scope.topReadLabels[i],
                        style: 'tableData',
                        alignment: 'left'
                    },
                    {   
                        //toString, idk why pero ayaw magdisplay ng int na data
                        text: ($scope.topReadData[i] != null ? $scope.topReadData[i].toString() : "0"),
                        style: 'tableData',
                        alignment: 'center'
                    }
                ];

                body.push(row);
            }
        }

        return body;
    };


    $scope.createMostFavoriteTable = function () {

        //Header
        var body = [
            [
                { text: 'Most Favorited Manga', style: 'tableHeader', alignment: 'center' },
                { text: 'Fave Count', style: 'tableHeader', alignment: 'center' }
            ]
        ];

        //check
        if ($scope.topMangaLabels && $scope.topMangaData) {
            //loop to create table rows
            for (var i = 0; i < $scope.topMangaLabels.length; i++) {

                var row = [
                    {
                        //Manga Title
                        text: $scope.topMangaLabels[i],
                        style: 'tableData',
                        alignment: 'left'
                    },
                    {
                        //Fave Count, ToString
                        text: ($scope.topMangaData[i] != null ? $scope.topMangaData[i].toString() : "0"),
                        style: 'tableData',
                        alignment: 'center'
                    }
                ];

                body.push(row);
            }
        }

        return body;
    };


    $scope.createNewUsersTable = function () {
        //Initialize Container
        var totalNewUsers = 0;

        // Check 
        if ($scope.newUsersData && $scope.newUsersData.length > 0) {

            // hehehehe kabognatan
            // no time to change, marami nang dependencies, nareturn ko kasi is array within an array hindi array lang
            var dataList = $scope.newUsersData[0];

            if (dataList) {
                for (var i = 0; i < dataList.length; i++) {
                    totalNewUsers += dataList[i];
                }
            }
        }

        // Body
        return [
            [
                { text: 'Period', style: 'tableHeader', alignment: 'center' },
                { text: 'Total New Users', style: 'tableHeader', alignment: 'center' }
            ],
            [
                { text: 'Last 30 Days', style: 'tableData', alignment: 'left' },
                { text: totalNewUsers.toString(), style: 'tableData', alignment: 'center' }
            ]
        ];
    };


    $scope.createDailyLoginsTable = function () {
        var totalLogins = 0;

        // Check 
        if ($scope.dailyLoginData && $scope.dailyLoginData.length > 0) {

            // hehehehe kabognatan
            var dataList = $scope.dailyLoginData[0];

            if (dataList) {
                for (var i = 0; i < dataList.length; i++) {
                    totalLogins += dataList[i];
                }
            }
        }

        // Body
        return [
            [
                { text: 'Period', style: 'tableHeader', alignment: 'center' },
                { text: 'Total Daily Logins', style: 'tableHeader', alignment: 'center' }
            ],
            [
                { text: 'Last 30 Days', style: 'tableData', alignment: 'left' },
                { text: totalLogins.toString(), style: 'tableData', alignment: 'center' }
            ]
        ];
    };

    $scope.createMostGenreTable = function () {

        //Header
        var body = [
            [
                { text: 'Most Favorited Manga', style: 'tableHeader', alignment: 'center' },
                { text: 'Fave Count', style: 'tableHeader', alignment: 'center' }
            ]
        ];

        //check
        if ($scope.topGenreLabels && $scope.topGenreData) {
            //loop to create table rows
            for (var i = 0; i < $scope.topGenreLabels.length; i++) {

                var row = [
                    {
                        //Genre Title
                        text: $scope.topGenreLabels[i],
                        style: 'tableData',
                        alignment: 'left'
                    },
                    {
                        //Fave Count, ToString
                        text: ($scope.topGenreData[i] != null ? $scope.topGenreData[i].toString() : "0"),
                        style: 'tableData',
                        alignment: 'center'
                    }
                ];

                body.push(row);
            }
        }

        return body;
    };


    $scope.downloadAnalytics = function () {
        //Playground

        //No Special Symbols for fileneames
        //Reference for pdf and filename
        var today = new Date();
        //String Builder (Ofc may help)
        var filenameDate = `${(today.getMonth() + 1).toString().padStart(2, '0')}/${today.getDate().toString().padStart(2, '0')}/${today.getFullYear()}`;


        var dd = {
            pageMargins: [40, 40, 40, 40],
            content: [
                // Header
                {
                    stack: [
                        {
                            image: 'data:image/png;base64,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',
                            width: 70,
                            alignment: 'center'
                        },
                        {
                            text: 'MangaLab Analytics Report',
                            style: 'header',
                            alignment: 'center'
                        },
                        {
                            text: `Performance Overview (${filenameDate})`,
                            style: 'subheader',
                            alignment: 'center'
                        },
                        {
                            canvas: [{ type: 'line', x1: 0, y1: 5, x2: 515, y2: 5, lineWidth: 1 }],
                            margin: [0, 10, 0, 20]
                        }
                    ]
                },

                // Tables ( In Columns )
                {
                    columns: [
                       
                        {
                            width: '*', //half the page
                            stack: [
                                //Most Favorited Genre
                                {
                                    table: {
                                        headerRows: 1,
                                        
                                        widths: ['*', 'auto'],
                                        body: $scope.createMostGenreTable()
                                    }
                                },

                                { text: '\n' },

                                // New Users (Count)
                                {
                                    table: {
                                        headerRows: 1,
                                        widths: ['*', 'auto'],
                                        body: $scope.createNewUsersTable()
                                    }
                                },

                                { text: '\n' },

                                // Monthly Logins (Count)
                                {
                                    table: {
                                        headerRows: 1,
                                        widths: ['*', 'auto'],
                                        body: $scope.createDailyLoginsTable()
                                    }
                                }
                            ]
                        },

                        // Gap
                        { width: 25, text: '' },

                        // Manga
                        {
                            width: '*',
                            stack: [
                                // Most Favorited Manga
                                {
                                    table: {
                                        headerRows: 1,
                                        widths: ['*', 'auto'],
                                        body: $scope.createMostFavoriteTable()
                                    }
                                },

                                { text: '\n' },

                                // Most Read
                                {
                                    table: {
                                        headerRows: 1,
                                        widths: ['*', 'auto'],
                                        //Table Generator Function
                                        body: $scope.createMostReadTable()
                                    }
                                }
                            ]
                        }
                    ]
                }
            ],

            // Footer
            footer: {
                margin: [40, 10, 40, 0],
                stack: [
                    {
                        canvas: [
                            { type: 'line', x1: 0, y1: 0, x2: 515, y2: 0, lineWidth: 1, lineColor: '#000000' }
                        ],
                        margin: [0, 0, 0, 5]
                    },
                    {
                        columns: [
                            {
                                text: 'MangaLab Analytics Report',
                                alignment: 'left',
                                fontSize: 9,
                                color: '#000000'
                            },
                            {
                                text: 'Administrators Use Only',
                                alignment: 'right',
                                fontSize: 9,
                                color: '#000000'
                            }
                        ]
                    }
                ]
            },

            styles: {
                header: { fontSize: 25, bold: true, color: '#333333', margin: [0, 0, 0, 5] },
                subHeader: { fontSize: 10, color: 'black', alignment: 'center', margin: [0, 0, 0, 5] },
                tableHeader: { fillColor: 'darkred', color: 'white', bold: true, fontSize: 10, margin: [0, 5, 0, 5] },
                tableData: { fontSize: 10, color: '#333333', margin: [0, 5, 0, 0] }
            }
        };

        

        
        pdfMake.createPdf(dd).download(`MangaLabAnalytics_${filenameDate}.pdf`);


    };










    

    //#################################################### STATISTICS (CHARTS & TABLES) PAGE ####################################################










    //#################################################### REDIRECTIONS ####################################################


    $scope.redirectToMangaList = function () {
        window.location.href = "/MangaLab/MangaListPage";
    }

    $scope.redirectToHome = function () {
        window.location.href = "/MangaLab/HomePage";
    }

    $scope.redirectToSignIn = function () {
        window.location.href = "/MangaLab/SignInPage";
    }

    $scope.redirectToRegister = function () {
        window.location.href = "/MangaLab/RegisterPage";
    }

    $scope.redirectToAccount = function () {
        window.location.href = "/MangaLab/AccountPage";
    }

    $scope.redirectToAdministration = function () {
        window.location.href = "/MangaLab/AdministrationPage";
    }


    $scope.redirectToManga = function (manga) {
        var mangaPageUrl = '/MangaLab/MangaPage?id=' + manga.id;
        window.location.href = mangaPageUrl;
    };

    $scope.redirectToRead = function (chapter) {
        if (!chapter || !chapter.id) {
            console.error("Invalid chapter data provided.");
            return;
        }
        
        const mangaId = $scope.mangaDetails.id;

        const chapterId = chapter.id;

        if (!mangaId) {
            console.error("Manga ID is missing from the scope.");
            return;
        }

        window.location.href = `/MangaLab/ReadPage?mangaId=${mangaId}&chapterId=${chapterId}`;
    };

    $scope.search = { query: '' };
    $scope.redirectToSearchResult = function () {

        console.log("Search submitted with query:", $scope.search.query);

        if ($scope.search.query) {
            $window.location.href = '/MangaLab/SearchResultPage?query=' + encodeURIComponent($scope.search.query);
        }
    };

    $scope.redirectToContinueReading = function (historyItem) {
        if (!historyItem || !historyItem.manga || !historyItem.chapterId) {
            console.error("Invalid history item provided.");
            return;
        }

        const mangaId = historyItem.manga.id;
        const chapterId = historyItem.chapterId;

        window.location.href = `/MangaLab/ReadPage?mangaId=${mangaId}&chapterId=${chapterId}`;
    };

    //#################################################### REDIRECTIONS ####################################################





    //#################################################### SEARCH PAGE ####################################################

    $scope.mangaList = [];
    $scope.searchQuery = '';
    $scope.isLoading = true;

    $scope.initSearch = function (query) {
        if (!query) {
            $scope.isLoading = false;
            return;
        }

        $scope.searchQuery = query;
        MangaLabService.searchManga(query).then(function (response) {
            var mangas = response.data.data;

            mangas.forEach(function (manga) {
                var coverArt = manga.relationships.find(rel => rel.type === 'cover_art');
                var coverFileName = coverArt ? coverArt.attributes.fileName : '';
                manga.coverUrl = `https://uploads.mangadex.org/covers/${manga.id}/${coverFileName}`;
            });

            $scope.mangaList = mangas;
        }).finally(function () {
            $scope.isLoading = false;
        });
    };

    //#################################################### SEARCH PAGE ####################################################




    //#################################################### MANGALIST PAGE ####################################################
    //Initial Storage 
    $scope.mangaList = [];
    //Spinner handler
    $scope.isLoading = true;

    //Filters
    $scope.selectedGenre = null;
    $scope.selectedStatus = null;
    $scope.selectedOrder = 'order[followedCount]=desc';
    $scope.selectedOrderLabel = 'Most Follows';

    
    $scope.setOrder = function (orderValue, orderLabel) {

        $scope.selectedOrder = orderValue;
        $scope.selectedOrderLabel = orderLabel;
        $scope.loadManga(); 
    };

    $scope.loadManga = function () {
        $scope.isLoading = true;

        MangaLabService.getMangaList($scope.selectedGenre, $scope.selectedStatus, $scope.selectedOrder)
            .then(function (response) {

                $scope.mangaList = response.data.data; 

                $scope.mangaList.forEach(function (manga) {
                    const titles = manga.attributes.title;
                    manga.displayTitle = titles.en || titles['ja-ro'] || Object.values(titles)[0] || "Title Not Available";

                    var coverArt = manga.relationships.find(rel => rel.type === 'cover_art');
                    var coverFileName = coverArt ? coverArt.attributes.fileName : '';
                    manga.coverUrl = `https://uploads.mangadex.org/covers/${manga.id}/${coverFileName}`;
                });

            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.loadManga();

    //#################################################### MANGALIST PAGE ####################################################




    //#################################################### RECENTLY ADDED ####################################################
    $scope.recentlyAddedList = [];
    $scope.loadRecentlyAdded = function () {
        $scope.isLoading = true;
        MangaLabService.getRecentlyAddedList().then(function (response) {
            var rawMangaData = response.data.data;

            $scope.recentlyAddedList = rawMangaData.map(function (manga) {
                const titles = manga.attributes.title;
                manga.displayTitle = titles.en || titles['ja-ro'] || Object.values(titles)[0] || "Title Not Available";

                var coverArt = manga.relationships.find(function (rel) {
                    return rel.type === 'cover_art';
                });
                var coverFileName = coverArt ? coverArt.attributes.fileName : '';
                manga.coverUrl = `https://uploads.mangadex.org/covers/${manga.id}/${coverFileName}`;

                return manga;
            });
            $scope.isLoading = false;

        });
    };
    //#################################################### RECENTLY ADDED ####################################################



    //#################################################### MANGA/DETAIL PAGE ####################################################
    $scope.initMangaPage = function (mangaId) {
        //AYAW AISDJIOASDJOIASDK
        console.log("initMangaPage received this ID:", mangaId);
        //save for swagger's favorite
        $scope.currentMangaID = mangaId;
        checkFavoriteStatus();

        if (!mangaId) { return; }
        $scope.detailsAreLoading = true;
        MangaLabService.getMangaDetails(mangaId).then(function (response) {

            $scope.mangaDetails = response.data;

            if ($scope.mangaDetails && $scope.mangaDetails.attributes) {

                if ($scope.mangaDetails.attributes.tags) {
                    const genreIds = $scope.mangaDetails.attributes.tags
                        .filter(tag => tag.attributes.group === 'genre')
                        .map(tag => tag.id)
                        .slice(0, 2); 

                    if (genreIds.length > 0) {
                        MangaLabService.getSimilarManga(mangaId, genreIds).then(function (similarResponse) {
                            $scope.similarManga = similarResponse.data.slice(0, 5);
                        });
                    }
                }

                const description = $scope.mangaDetails.attributes.description || {};
                $scope.displayDescription = description.en || Object.values(description)[0] || "No description available.";

                const title = $scope.mangaDetails.attributes.title || {};
                $scope.displayTitle = title.en || Object.values(title)[0] || "No Title Available";


                if ($scope.mangaDetails.statistics && $scope.mangaDetails.statistics.follows) {
                    $scope.followCount = $scope.mangaDetails.statistics.follows;
                } else {
                    $scope.followCount = 0;
                }

                if ($scope.mangaDetails.attributes.tags) {
                    $scope.genres = $scope.mangaDetails.attributes.tags.filter(function (tag) {
                        return tag.attributes.group === 'genre';
                    });
                } else {
                    $scope.genres = []; 
                }

                var coverArt = $scope.mangaDetails.relationships.find(rel => rel.type === 'cover_art');
                var author = $scope.mangaDetails.relationships.find(rel => rel.type === 'author');
                var coverFileName = coverArt ? coverArt.attributes.fileName : '';
                $scope.coverUrl = `https://uploads.mangadex.org/covers/${$scope.mangaDetails.id}/${coverFileName}`;
                $scope.authorName = author ? author.attributes.name : 'Unknown';

            } else {
                console.error("Failed to get valid manga attributes for ID:", mangaId);
                $scope.displayDescription = "This manga's data could not be loaded.";
                $scope.authorName = "Unknown";
                $scope.followCount = 0;
                $scope.genres = [];
            }

            $scope.detailsAreLoading = false;
        });
    };

    $scope.getCoverUrl = function (manga) {
        if (!manga || !manga.relationships) return ''; 
        const coverArt = manga.relationships.find(rel => rel.type === 'cover_art');
        if (coverArt && coverArt.attributes) {
            const fileName = coverArt.attributes.fileName;
            return `https://uploads.mangadex.org/covers/${manga.id}/${fileName}`;
        }
        return ''; 
    };
    //#################################################### MANGA/DETAIL PAGE ####################################################



    //#################################################### READ PAGE ####################################################
    $scope.initReadPage = function (mangaId, chapterId) {

        if (mangaId && chapterId) {
            MangaLabService.updateReadingHistory(mangaId, chapterId).then(function (response) {
                    console.log("Reading history updated.");
                })
                .catch(function (error) {
                    console.error("Failed to update reading history:", error);
                });
        } else {
            console.warn("Not updating history");
        }

        $scope.pages = [];
        $scope.allChapters = [];
        $scope.mangaTitle = 'Loading...';
        $scope.currentChapter = null;
        $scope.previousChapter = null;
        $scope.nextChapter = null;
        $scope.isLoading = true; 
        $scope.errorMessage = '';


        MangaLabService.getReadingPages(mangaId, chapterId).then(function (response) {
                const { pageData, allChapters, mangaData } = response.data;

                //Manga Title
                if (mangaData && mangaData.attributes && mangaData.attributes.title) {
                    const title = mangaData.attributes.title;
                    $scope.mangaTitle = title.en || Object.values(title)[0] || "Manga Title";
                }

                if (pageData && pageData.baseUrl && pageData.chapter) {
                    const baseUrl = pageData.baseUrl;
                    $scope.pages = pageData.chapter.data.map(function (filename) {
                        return `${baseUrl}/data/${pageData.chapter.hash}/${filename}`;
                    });
                } else {
                    $scope.errorMessage = "Could not load chapter images.";
                }

                if (allChapters && allChapters.length > 0) {
                    $scope.allChapters = allChapters;
                    const currentIndex = allChapters.findIndex(c => c.id === chapterId);

                    if (currentIndex !== -1) {
                        $scope.currentChapter = allChapters[currentIndex];
                        $scope.previousChapter = (currentIndex + 1 < allChapters.length) ? allChapters[currentIndex + 1] : null;
                        $scope.nextChapter = (currentIndex > 0) ? allChapters[currentIndex - 1] : null;
                    }
                }

                $scope.mangaId = mangaId;

            })
            .finally(function () {
                $scope.isLoading = false;
            });
    };

    $scope.changeChapter = function (chapter) {
        if (!chapter || !$scope.mangaId) return;
        window.location.href = `/MangaLab/ReadPage?mangaId=${$scope.mangaId}&chapterId=${chapter.id}`;
    };

    //#################################################### READ PAGE ####################################################


});
