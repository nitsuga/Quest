begin transaction

	declare @sbps int=0;
	declare @msg varchar(200);
	select @sbps = count(*) from Destinations Where IsStandby = 1 and IsOld=0;
	if (@sbps<>118)
	begin
		set @msg = 'Unexpected number of standby points to remove. I was expecting 118 but there are ' + convert(varchar(200),@sbps);
		Print @msg
		RAISERROR (@msg,16, 1 );  
	end
	else
	begin

		-- mark all original standby points as old

		update Destinations set EndDate=GetDate() Where IsStandby = 1;

		update Destinations set Status='Uncovered' Where IsStandby = 1;

		insert into Destinations ([Shortcode],[Destination],[Wkt],[IsHospital],[IsStandby],[IsStation],[IsRoad],[IsPolice],[IsAandE],[Status],[LastUpdate],[StartDate],[EndDate])
		VALUES
		('LAS3301','Parsloes Avenue, Becontree'			,'POINT(547963 185474)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3302','Watford Way, Hendon'				,'POINT(522933 188577)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3303','Bromley Common, Bromley'			,'POINT(541279 168042)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3304','Whitehall, Charing Cross'			,'POINT(530043 180366)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3305','Hackney Road, Shoreditch'			,'POINT(533424 182673)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3306','Old Town, Croydon'					,'POINT(532046 165007)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3307','Western Avenue, Hanger Hill'		,'POINT(518401 182667)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3308','Hertford Road, Ponders End'			,'POINT(535275 196153)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3309','Station Road, Friern Barnet'		,'POINT(529186 191905)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3310','Plumstead High Street, Plumstead'	,'POINT(545309 178567)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3311','Sidcup Road, New Eltham'			,'POINT(543581 172755)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3312','Shepherds Bush Road, Hammersmith'	,'POINT(523370 178662)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3313','Monument Way, Tottenham'			,'POINT(533815 189513)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3314','Station Road, North Harrow'			,'POINT(513431 188676)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3315','Eastern Avenue, Romford'			,'POINT(550772 189882)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3316','Uxbridge Road, Yeading'				,'POINT(510985 180859)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3317','Bath Road, Hounslow'				,'POINT(512215 176140)',0,1,0,1,0,0,'Uncovered',GetDate(),GetDate(),null),
		('LAS3318','Camden Road, Holloway'				,'POINT(529891 185130)',0,1,0,1,0,0,'Uncovered',GetDate(),GetDate(),null),
		('LAS3319','Wheatfield Way, Kingston upon Thames','POINT(51819 169039)',0,1,0,1,0,0,'Uncovered',GetDate(),GetDate(),null),
		('LAS3320','Brixton Road, Brixton'				,'POINT(531056 175512)',0,1,0,1,0,0,'Uncovered',GetDate(),GetDate(),null),
		('LAS3321','Kirkdale, Sydenham'					,'POINT(535240 171604)',0,1,0,1,0,0,'Uncovered',GetDate(),GetDate(),null),
		('LAS3322','Loampit Vale, Lewisham'				,'POINT(537889 175817)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3323','Barking Road, East Ham'				,'POINT(542434 183527)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3324','Eastern Avenue, Gants Hill'			,'POINT(543264 188418)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3325','Old Kent Road, Newington'			,'POINT(533508 178436)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3326','Rosehill Roundabout, Rosehill'		,'POINT(526040 166561)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3327','East India Dock Road, Limehouse'	,'POINT(537254 181046)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3328','High Road, Leyton'					,'POINT(538241 186115)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3329','Tooting Road, Tooting Bec'			,'POINT(527907 172416)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3330','Elgin Avenue, Maida Vale'			,'POINT(524864 182121)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3531','Harrow Manor Way, Abbey Wood'		,'POINT(547346 179805)',0,1,0,1,0,0,'Actioned',GetDate(),GetDate(),null),
		('LAS3532','Kilburn High Road, Kilburn'			,'POINT(525181 183946)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3533','Kew Road, Kew'						,'POINT(519011 177638)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3534','High Street, Ruislip'				,'POINT(509243 187366)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3535','London Road, Thornton Heath'		,'POINT(531007 168555)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3536','Cale Street, Chelsea'				,'POINT(527366 178439)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null),
		('LAS3537','Lavender Avenue, Mitcham'			,'POINT(527481 169624)',0,1,0,1,0,0,'Covered',GetDate(),GetDate(),null)


	end
commit transaction
