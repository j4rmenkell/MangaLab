app.service("MangaLabService", function ($http) {
    // REGISTRATION

    this.saveRegis = function (userInfo) {
        var response = $http({
            method: "post",
            url: '/MangaLab/RegisterUser',
            data: userInfo
        });
        return response;
    };

    // LOGIN / SESSION 
    this.login = function (userInfo) {
        var response = $http({
            method: "post",
            url: '/MangaLab/LoginUser',
            data: userInfo
        });
        return response;
    };
    this.logout = function () {
        return $http({
            method: "post",
            url: '/MangaLab/Logout'
        });
    };
    this.getLoginStatus = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetLoginStatus'
        });
    };
    this.getAccountStats = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetAccountStats'
        });
    };

    // --- ACCOUNT / PROFILE ---
    this.saveEdit = function (formData) {
        var response = $http({
            method: "post",
            url: '/MangaLab/SaveEditProfile',
            data: formData,
            headers: {
                'Content-Type': undefined
            },
            transformRequest: angular.identity
        });
        return response;
    };


    this.updateEmail = function (currentPassword, newEmail) {
        return $http({
            method: 'POST',
            url: '/MangaLab/ChangeEmail',
            data: {
                currentPassword: currentPassword,
                newEmail: newEmail
            }
        });
    };
    this.updatePassword = function (currentPassword, newPassword, confirmPassword) {
        return $http({
            method: 'POST',
            url: '/MangaLab/ChangePassword',
            data: {
                currentPassword: currentPassword,
                newPassword: newPassword,
                confirmPassword: confirmPassword
            }
        });
    };


    // --- FAVORITES ---
    this.addFavorite = function (mangaID) { 
        return $http({
            method: 'POST',
            url: '/MangaLab/AddFavorite',
            data: {
                mangaID: mangaID
            }
        });
    };
    this.isFavorite = function (mangaID) { 
        return $http({
            method: 'GET',
            url: '/MangaLab/IsFavorite',
            params: {
                mangaID: mangaID
            }
        });
    };
    this.archiveFavoriteByMangaID = function (mangaID) { 
        return $http({
            method: 'POST',
            url: '/MangaLab/ArchiveFavoriteByMangaID',
            data: {
                mangaID: mangaID
            }
        });
    };
    this.getFavoriteManga = function () { 
        return $http({
            method: 'GET',
            url: '/MangaLab/GetFavoriteManga'
        });
    };
    this.archiveFavoriteItem = function (favoriteID) { 
        return $http({
            method: 'POST',
            url: '/MangaLab/ArchiveFavoriteItem',
            data: {
                favoriteID: favoriteID
            }
        });
    };

    // --- HISTORY / READING ---
    this.getContinueReading = function () { 
        return $http({
            method: 'GET',
            url: '/MangaLab/GetContinueReading'
        });
    };
    this.archiveHistoryItem = function (historyID) { 
        return $http({
            method: 'POST',
            url: '/MangaLab/ArchiveHistoryItem',
            data: {
                historyID: historyID
            }
        });
    };
    this.updateReadingHistory = function (mangaID, chapterID) { 
        return $http({
            method: 'POST',
            url: '/MangaLab/UpdateReadingHistory',
            data: {
                mangaID: mangaID,
                chapterID: chapterID
            }
        });
    };

    // --- RECOMMENDATIONS ---
    this.getRecommendedManga = function (genreId) { 
        return $http({
            method: 'GET',
            url: '/MangaLab/GetRecommendedMangaByGenre',
            params: {
                genreId: genreId
            }
        });
    };
    
    this.getAccountStats = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetAccountStats'
        });
    };

    // --- API ---

    var backendUrl = 'http://localhost:44312'; 

    this.getMangaList = function () {
        return $http({
            method: 'GET',
            url: backendUrl + '/MangaLab/GetMangaList'
        });
    };

    this.getRecentlyAddedList = function () {
        return $http({
            method: 'GET',
            url: backendUrl + '/MangaLab/GetRecentlyAddedList'
        });
    };

    this.getMangaDetails = function (mangaId) {
        return $http({
            method: 'GET',
            url: backendUrl + '/MangaLab/GetMangaDetails?mangaId=' + mangaId
        });
    };

    this.searchManga = function (query) {
        return $http({
            method: 'GET',
            url: backendUrl + '/MangaLab/SearchMangaApi?query=' + encodeURIComponent(query)
        });
    };

    this.getSimilarManga = function (currentMangaId, genreIds) {
        const genreQueryString = genreIds.map(id => `genreIds=${id}`).join('&');

        return $http({
            method: 'GET',
            url: `${backendUrl}/MangaLab/GetSimilarManga?currentMangaId=${currentMangaId}&${genreQueryString}`
        });
    };

    this.getReadingPages = function (mangaId, chapterId) {
        const url = backendUrl + '/MangaLab/GetReadingPages';

        const params = {
            mangaId: mangaId,
            chapterId: chapterId
        };

        return $http.get(url, { params: params });
    };

    this.getRecommendedManga = function (genreId) {     
        return $http({
            method: 'GET',
            url: '/MangaLab/GetRecommendedMangaByGenre',
            params: {
                genreId: genreId
            }
        });
    };

    this.getMangaList = function (genre, status, order) {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetMangaList',
            params: {
                genre: genre,
                status: status,
                order: order
            }
        });
    };

    // --- CHARTS ---

    this.getTopFavoriteMangaStats = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetTopFavoriteMangaStats'
            });
    };

    this.getTopReadMangaStats = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetTopReadMangaStats'
        });
    };

    this.getNewUsers = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetNewUsers'
        });
    };

    this.getDailyLoginStats = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetDailyLoginStats'
        });
    };

    this.getAccountsList = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetAccountList'
        });
    };

    this.getLogsList = function () {
        return $http({
            method: 'GET',
            url: '/MangaLab/GetLogsList'
        });
    };

    this.archiveAccount = function (registrationID) {
        return $http.post('/MangaLab/ArchiveAccount', {
            registrationID: registrationID
        });
    };

    this.updateAccountStatus = function (registrationID, newStatus) {
        return $http({
            method: 'GET',
            url: '/MangaLab/UpdateAccountStatus',
            params: {
                registrationID: registrationID,
                newStatus: newStatus    
            }
        });
    };

    this.getRoleList = function () {
        return $http.get('/MangaLab/GetRoleList');
    };

    this.updateUserRole = function (regID, newID) {
        return $http.post('/MangaLab/UpdateUserRole', {
            registrationID: regID,
            newRoleID: newID
        });
    };

    



    //this.getArchivedList = function () {
    //    return $http({
    //        method: 'GET',
    //        url: '/MangaLab/GetArchivedList'
    //    });
    //};

    //this.getLogsList = function () {
    //    return $http({
    //        method: 'GET',
    //        url: '/MangaLab/GetLogsList'
    //    });
    //};

});