using System;
using System.IO;
using System.Text;

namespace Common.IO;

public static class TemporaryFile
{
	private	const	string	TEMPORARYNAME_CHARS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private	const	int		TEMPORARYNAME_LENGTH = 8;

	public	static	string	CreateTemporaryFileName(string directory, string ext)
	{
		return Path.GetFileName(CreateTemporaryPath(directory, ext));
	}

	public	static	string	CreateTemporaryFileName(string directory)
	{
		return Path.GetFileName(CreateTemporaryPath(directory));
	}

	public	static	string	CreateTemporaryPath(string? directory, string ext)
	{
		if(string.IsNullOrEmpty(directory)){
			directory = Path.GetTempPath();
		}

		Random			rnd = new Random();

		while(true){
			string	path = Path.Combine(directory, CreateRandomName(rnd) + ext);
			if(!PathExtensions.Exists(path))	return path;
		}
	}

	public	static	string	CreateTemporaryPath(string? directory)
	{
		return CreateTemporaryPath(directory, ".tmp");
	}

	private	static	string	CreateRandomName(Random rnd)
	{
		StringBuilder	sb = new StringBuilder(TEMPORARYNAME_LENGTH);

		for(int i = 0; i < TEMPORARYNAME_LENGTH; i++){
			int		pos = rnd.Next(TEMPORARYNAME_CHARS.Length);
			char	c = TEMPORARYNAME_CHARS[pos];
			sb.Append(c);
		}

		return sb.ToString();
	}
}
