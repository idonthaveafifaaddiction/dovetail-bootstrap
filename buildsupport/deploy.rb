NUGET_FEED = "\\\\marvin.fcs.local\\product\\nuget_feed"

#desc "Deploy nuget packages to local feed (share)"
task :deploy_nuget_packages do 
	Dir.glob(File.join("results//packages","*.nupkg")){ |file|
		FileUtils.cp file, NUGET_FEED
	}
end