namespace AdventureScript
{
    class CodeWriter
    {
        TextWriter m_writer;
        int m_indent = 0;
        bool m_inLine = false;

        public CodeWriter(TextWriter writer)
        {
            m_writer = writer;
        }

        public TextWriter TextWriter => m_writer;

        public void Write(string value)
        {
            WriteIndent();
            m_writer.Write(value);
        }

        public void EndLine()
        {
            if (m_inLine)
            {
                m_writer.WriteLine();
                m_inLine = false;
            }
        }

        public void BeginBlock()
        {
            EndLine();
            Write("{");
            m_indent++;
            EndLine();
        }

        public void EndBlock()
        {
            m_indent--;
            EndLine();
            Write("}");
            EndLine();
        }

        void WriteIndent()
        {
            if (!m_inLine)
            {
                for (int i = 0; i < m_indent; i++)
                {
                    m_writer.Write("    ");
                }
                m_inLine = true;
            }
        }
    }
}
