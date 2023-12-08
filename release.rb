# frozen_string_literal: true

require 'rubygems'
require 'json'
require 'zip'

MOD_NAME = 'HideMUI'
RELEASE_FOLDERS = {
    thunderstore: Dir.new(__dir__ + '/thunderstore'),
    nexus: Dir.new(__dir__ + '/nexus')
}.freeze
DLL_NAME = "#{MOD_NAME}.dll"
DLL_PATH = File.join(__dir__, 'bin', 'Debug', DLL_NAME)

raise "Solution not built or DLL file not found at '#{DLL_PATH}'." unless File.exist?(DLL_PATH)
raise ArgumentError, 'Missing the app parameter' unless (release_app = ARGV.first&.to_sym)

target_folder = RELEASE_FOLDERS[release_app].path

case release_app
when :thunderstore
    manifest = JSON.parse(File.read(target_folder + '/manifest.json')).freeze
    version = manifest['version_number']

    release_sepcific_filenames = %w[
        icon.png
        manifest.json
        README.md
    ].map
when :nexus
else
    raise ArgumentError, "'#{release_app}' is not a valid app."
end

zip_name = "#{MOD_NAME}_#{release_app.capitalize}-#{version.gsub('.', '_')}-#{Time.now.strftime('%y%m%d')}.zip"
zip_path = File.join(target_folder, 'releases', zip_name)
raise Exception, "Release #{version} for #{release_app.capitalize} already exists!" if File.exist?(zip_path)

Zip::File.open(zip_path, create: true) do |zipfile|
    zipfile.add(DLL_NAME, DLL_PATH)
    release_sepcific_filenames.each do |filename|
        zipfile.add(filename, File.join(target_folder, filename))
    end
end

puts "Successfully created #{release_app.capitalize} release under: '#{zip_path}'."
